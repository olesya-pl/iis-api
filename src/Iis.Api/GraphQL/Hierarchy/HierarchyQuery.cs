using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Resolvers;
using Iis.Domain;
using Iis.Domain.Users;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using Iis.Utility;
using IIS.Core.GraphQL;
using Newtonsoft.Json.Linq;

namespace Iis.Api.GraphQL.Hierarchy
{
    public class HierarchyQuery
    {
        private const string HeaderName = "entityMilitaryOrganization";
        private const string IdName = "id";
        private const string TitleName = "title";
        private const string TitleUnderscoreName = "__title";
        private const string MilitaryBaseShortName = "militaryBaseShortName";
        private const string TypeName = "__typeName";
        private const string AffiliationName = "affiliation";
        private const string SidcName = "sidc";
        private const string ClassifiersName = "classifiers";
        private const string CodeName = "code";
        private const string ParentName = "parent";
        private const string ChildName = "child";
        private const string PhotoName = "photo";
        private const string UrlName = "url";
        private const string CommonInfoName = "commonInfo";
        private const string OpenName = "OpenName";
        private const string RealNameShortName = "RealNameShort";

        public HierarchyResult GetHierarchy(
            IResolverContext ctx,
            [Service] IOntologyService ontologyService,
            [Service] ISecurityLevelChecker securityLevelChecker,
            Guid id)
        {
            var node = ontologyService.GetNode(id);
            var tokenPayload = ctx.GetToken();

            if (node == null || !HierarchyAccessGranted(tokenPayload.User, node.OriginalNode, securityLevelChecker))
            {
                throw new Exception($"{FrontEndErrorCodes.NotFound}");
            }

            var result = new HierarchyResult();
            result.Data = new JObject();
            result.Data[HeaderName] = GetHierarchyJson(node.OriginalNode, false, false, tokenPayload.User, securityLevelChecker);
            return result;
        }

        private static JObject GetHierarchyJson(INode node, bool parentShort, bool childShort, User user, ISecurityLevelChecker securityLevelChecker)
        {
            var result = new JObject();
            result[IdName] = node.Id;
            result[TitleName] = node.GetSingleDirectProperty(IdName)?.Value;
            result[TitleUnderscoreName] = node.GetTitleValue();
            result[MilitaryBaseShortName] = node.GetSingleDirectProperty(MilitaryBaseShortName)?.Value;
            result[TypeName] = node.NodeType.Name;

            var affiliation = new JObject();
            affiliation[SidcName] = node.GetSingleDirectProperty(AffiliationName)?.GetSingleDirectProperty(SidcName)?.Value;
            result[AffiliationName] = affiliation;

            var classifiers = new JObject();
            var classifiersSids = new JObject();
            classifiersSids[CodeName] = node.GetSingleDirectProperty(ClassifiersName)
                ?.GetSingleDirectProperty(SidcName)
                ?.GetSingleDirectProperty(CodeName)
                ?.Value;
            classifiers[SidcName] = classifiersSids;
            result[ClassifiersName] = classifiers;

            var commonInfo = new JObject();
            commonInfo[OpenName] = node.GetSingleDirectProperty(CommonInfoName)?.GetSingleDirectProperty(OpenName)?.Value;
            commonInfo[RealNameShortName] = node.GetSingleDirectProperty(CommonInfoName)?.GetSingleDirectProperty(RealNameShortName)?.Value;
            result[CommonInfoName] = commonInfo;

            result[PhotoName] = GetPhotoJson(node.GetSingleDirectProperty(PhotoName));

            var childArray = new JArray();
            var childRelations = node.GetIncomingRelations(new[] { ParentName });
            foreach (var childRelation in childRelations)
            {
                var childObj = childShort ?
                    GetShortJson(childRelation.SourceNode) :
                    GetHierarchyJson(childRelation.SourceNode, true, true, user, securityLevelChecker);
                childArray.Add(childObj);
            }
            result[ChildName] = childArray;

            var parentNode = node.GetSingleDirectProperty(ParentName);

            result[ParentName] = parentNode == null ? null :
                parentShort ? GetShortJson(parentNode) : GetHierarchyJson(parentNode, false, true, user, securityLevelChecker);
            return result;
        }

        private static bool HierarchyAccessGranted(User user, INode hierarchyNode, ISecurityLevelChecker securityLevelChecker)
        {
            var node = hierarchyNode;
            while (node != null)
            {
                if (!securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes, node.GetSecurityLevelIndexes()))
                {
                    return false;
                }

                node = node.GetSingleDirectProperty(ParentName);
            }
            return true;
        }

        private static JObject GetShortJson(INode node)
        {
            var result = new JObject();
            result[IdName] = node.Id;
            result[TypeName] = node.NodeType.Name;
            return result;
        }

        private static JObject GetPhotoJson(INode photoNode)
        {
            if (photoNode == null) return null;
            var result = new JObject();
            result[UrlName] = FileUrlGetter.GetFileUrl(new Guid(photoNode.Value));
            return result;
        }
    }
}
