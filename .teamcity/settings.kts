import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.dockerSupport
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.replaceContent
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.DotnetTestStep
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.ScriptBuildStep
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.script
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.finishBuildTrigger
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.vcs
import jetbrains.buildServer.configs.kotlin.v2019_2.vcs.GitVcsRoot

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2020.2"

project {

    params {
        param("teamcity.vcsTrigger.runBuildInNewEmptyBranch", "true")
    }

    subProject(Api)
    subProject(MaterialLoader)
}


object Api : Project({
    name = "API"

    vcsRoot(Api_IisContourUiHttps)
    vcsRoot(Api_IisNomad)

    buildType(Api_BuildDocker)
    buildType(Api_DeployIisDevNomad)

    params {
        param("DOCKER_IMAGE_NAME", "docker.contour.net:5000/iis-core")
        param("gitHashShort", "ffffffff")
    }
})

object Api_BuildDocker : BuildType({
    name = "Build Docker"

    params {
        param("env.CI_BUILD_VERSION", "%teamcity.build.branch%.%system.build.number%")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        script {
            name = "Register variables"
            scriptContent = """
                #!/bin/bash
                
                GIT_HASH=%build.vcs.number%
                GIT_HASH_SHORT=${'$'}{GIT_HASH:0:8}
                echo "##teamcity[setParameter name='gitHashShort' value='${'$'}{GIT_HASH_SHORT}']"
            """.trimIndent()
        }
        dotnetTest {
            name = "Unit tests"
            projects = "src/Iis.UnitTests/Iis.UnitTests.csproj"
            dockerImagePlatform = DotnetTestStep.ImagePlatform.Linux
            dockerImage = "mcr.microsoft.com/dotnet/core/sdk:3.1"
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.DEFAULT%")
        }
        dockerCommand {
            name = "Docker Build"
            commandType = build {
                source = file {
                    path = "Dockerfile"
                }
                namesAndTags = "%DOCKER_IMAGE_NAME%:%gitHashShort%"
                commandArgs = "--build-arg BUILD_VERSION=%env.CI_BUILD_VERSION%"
            }
        }
        dockerCommand {
            name = "Tag(latest)"

            conditions {
                equals("teamcity.build.branch.is_default", "true")
            }
            commandType = other {
                subCommand = "tag"
                commandArgs = "%DOCKER_IMAGE_NAME%:%gitHashShort% %DOCKER_IMAGE_NAME%:latest"
            }
        }
        dockerCommand {
            name = "Tag(branch and tag)"
            commandType = other {
                subCommand = "tag"
                commandArgs = "%DOCKER_IMAGE_NAME%:%gitHashShort% %DOCKER_IMAGE_NAME%:%teamcity.build.branch%"
            }
        }
        dockerCommand {
            name = "Docker Push"
            commandType = push {
                namesAndTags = """
                    %DOCKER_IMAGE_NAME%:%gitHashShort%
                    %DOCKER_IMAGE_NAME%:%teamcity.build.branch%
                """.trimIndent()
            }
        }
        dockerCommand {
            name = "Docker Push Tag(latest)"

            conditions {
                equals("teamcity.build.branch.is_default", "true")
            }
            commandType = push {
                namesAndTags = "%DOCKER_IMAGE_NAME%:latest"
            }
        }
    }

    triggers {
        vcs {
            triggerRules = "-:root=${Api_IisContourUiHttps.id}:.teamcity/*"

            branchFilter = """
                +:<default>
                +:v*
            """.trimIndent()
        }
    }

    features {
        dockerSupport {
            cleanupPushedImages = true
            loginToRegistry = on {
                dockerRegistryId = "PROJECT_EXT_2"
            }
        }
    }
})

object Api_DeployIisDevNomad : BuildType({
    name = "Deploy(iis-dev nomad)"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    maxRunningBuilds = 1

    params {
        param("env.NOMAD_ADDR", "http://is-dev-srv1.contour.net:4646")
        param("env.CONSUL_HTTP_ADDR", "http://is-dev-srv1.contour.net:8500")
        param("JOB_HCl", "iis_core.hcl")
        select("NOMAD_ENV", "dev", label = "NOMAD_ENV", description = "Nomad Environment",
                options = listOf("dev", "dev2", "dev3", "qa", "demo"))
    }

    vcs {
        root(Api_IisNomad)
    }

    steps {
        script {
            name = "Add tags"
            scriptContent = """curl -X POST -H "Content-Type: text/plain" --data "%NOMAD_ENV%" -u "%system.teamcity.auth.userId%:%system.teamcity.auth.password%" "%teamcity.serverUrl%/httpAuth/app/rest/builds/id:%teamcity.build.id%/tags/""""
        }
        script {
            name = "Nomad plan"
            scriptContent = """
                #!/bin/sh
                levant plan -ignore-no-changes iis-dev/%NOMAD_ENV%/%JOB_HCl%
            """.trimIndent()
            dockerImagePlatform = ScriptBuildStep.ImagePlatform.Linux
            dockerPull = true
            dockerImage = "docker.contour.net:5000/levant:0.3.0-beta1"
        }
        script {
            name = "Nomad run"
            scriptContent = """
                #!/bin/sh
                levant deploy -force -ignore-no-changes iis-dev/%NOMAD_ENV%/%JOB_HCl%
            """.trimIndent()
            dockerImagePlatform = ScriptBuildStep.ImagePlatform.Linux
            dockerPull = true
            dockerImage = "docker.contour.net:5000/levant:0.3.0-beta1"
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${Api_BuildDocker.id}"
            successfulOnly = true
        }
    }

    features {
        replaceContent {
            fileRules = "+:iis-dev/%NOMAD_ENV%/%JOB_HCl%"
            pattern = "%DOCKER_IMAGE_NAME%:latest"
            replacement = "%DOCKER_IMAGE_NAME%:${Api_BuildDocker.depParamRefs["gitHashShort"]}"
        }
    }

    dependencies {
        snapshot(Api_BuildDocker) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
    }
})

object Api_IisContourUiHttps : GitVcsRoot({
    name = "IIS/contour-ui(https)"
    url = "https://git.warfare-tec.com/IIS/contour-ui.git"
    branch = "develop"
    branchSpec = """
        +:refs/heads/*
        +:refs/tags/*
    """.trimIndent()
    useTagsAsBranches = true
    authMethod = password {
        userName = "tc_contour"
        password = "credentialsJSON:33e32587-317c-4e6a-8230-ad7ce5143a2e"
    }
})

object Api_IisNomad : GitVcsRoot({
    name = "IIS/nomad"
    url = "git@git.warfare-tec.com:IIS/nomad.git"
    branch = "master"
    useTagsAsBranches = true
    authMethod = uploadedKey {
        uploadedKey = "tc_contour"
    }
})


object MaterialLoader : Project({
    name = "Material-Loader"

    vcsRoot(MaterialLoader_IisContourUiHttps)
    vcsRoot(MaterialLoader_IisNomad)

    buildType(MaterialLoader_BuildDocker)
    buildType(MaterialLoader_DeployIisDevNomad)

    params {
        param("DOCKER_IMAGE_NAME", "docker.contour.net:5000/iis-core")
        param("gitHashShort", "ffffffff")
    }
})

object MaterialLoader_BuildDocker : BuildType({
    name = "Build Docker"

    params {
        param("env.CI_BUILD_VERSION", "%teamcity.build.branch%.%system.build.number%")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        script {
            name = "Register variables"
            scriptContent = """
                #!/bin/bash
                
                GIT_HASH=%build.vcs.number%
                GIT_HASH_SHORT=${'$'}{GIT_HASH:0:8}
                echo "##teamcity[setParameter name='gitHashShort' value='${'$'}{GIT_HASH_SHORT}']"
            """.trimIndent()
        }
        dotnetTest {
            name = "Unit tests"
            projects = "src/Iis.UnitTests/Iis.UnitTests.csproj"
            dockerImagePlatform = DotnetTestStep.ImagePlatform.Linux
            dockerImage = "mcr.microsoft.com/dotnet/core/sdk:3.1"
            param("dotNetCoverage.dotCover.home.path", "%teamcity.tool.JetBrains.dotCover.CommandLineTools.DEFAULT%")
        }
        dockerCommand {
            name = "Docker Build"
            commandType = build {
                source = file {
                    path = "Dockerfile"
                }
                namesAndTags = "%DOCKER_IMAGE_NAME%:%gitHashShort%"
                commandArgs = "--build-arg BUILD_VERSION=%env.CI_BUILD_VERSION%"
            }
        }
        dockerCommand {
            name = "Tag(latest)"

            conditions {
                equals("teamcity.build.branch.is_default", "true")
            }
            commandType = other {
                subCommand = "tag"
                commandArgs = "%DOCKER_IMAGE_NAME%:%gitHashShort% %DOCKER_IMAGE_NAME%:latest"
            }
        }
        dockerCommand {
            name = "Tag(branch and tag)"
            commandType = other {
                subCommand = "tag"
                commandArgs = "%DOCKER_IMAGE_NAME%:%gitHashShort% %DOCKER_IMAGE_NAME%:%teamcity.build.branch%"
            }
        }
        dockerCommand {
            name = "Docker Push"
            commandType = push {
                namesAndTags = """
                    %DOCKER_IMAGE_NAME%:%gitHashShort%
                    %DOCKER_IMAGE_NAME%:%teamcity.build.branch%
                """.trimIndent()
            }
        }
        dockerCommand {
            name = "Docker Push Tag(latest)"

            conditions {
                equals("teamcity.build.branch.is_default", "true")
            }
            commandType = push {
                namesAndTags = "%DOCKER_IMAGE_NAME%:latest"
            }
        }
    }

    triggers {
        vcs {
            triggerRules = "-:root=${MaterialLoader_IisContourUiHttps.id}:.teamcity/*"

            branchFilter = """
                +:<default>
                +:v*
            """.trimIndent()
        }
    }

    features {
        dockerSupport {
            cleanupPushedImages = true
            loginToRegistry = on {
                dockerRegistryId = "PROJECT_EXT_2"
            }
        }
    }
})

object MaterialLoader_DeployIisDevNomad : BuildType({
    name = "Deploy(iis-dev nomad)"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    maxRunningBuilds = 1

    params {
        param("env.NOMAD_ADDR", "http://is-dev-srv1.contour.net:4646")
        param("env.CONSUL_HTTP_ADDR", "http://is-dev-srv1.contour.net:8500")
        param("JOB_HCl", "iis_core.hcl")
        select("NOMAD_ENV", "dev", label = "NOMAD_ENV", description = "Nomad Environment",
                options = listOf("dev", "dev2", "dev3", "qa", "demo"))
    }

    vcs {
        root(MaterialLoader_IisNomad)
    }

    steps {
        script {
            name = "Add tags"
            scriptContent = """curl -X POST -H "Content-Type: text/plain" --data "%NOMAD_ENV%" -u "%system.teamcity.auth.userId%:%system.teamcity.auth.password%" "%teamcity.serverUrl%/httpAuth/app/rest/builds/id:%teamcity.build.id%/tags/""""
        }
        script {
            name = "Nomad plan"
            scriptContent = """
                #!/bin/sh
                levant plan -ignore-no-changes iis-dev/%NOMAD_ENV%/%JOB_HCl%
            """.trimIndent()
            dockerImagePlatform = ScriptBuildStep.ImagePlatform.Linux
            dockerPull = true
            dockerImage = "docker.contour.net:5000/levant:0.3.0-beta1"
        }
        script {
            name = "Nomad run"
            scriptContent = """
                #!/bin/sh
                levant deploy -force -ignore-no-changes iis-dev/%NOMAD_ENV%/%JOB_HCl%
            """.trimIndent()
            dockerImagePlatform = ScriptBuildStep.ImagePlatform.Linux
            dockerPull = true
            dockerImage = "docker.contour.net:5000/levant:0.3.0-beta1"
        }
    }

    triggers {
        finishBuildTrigger {
            buildType = "${MaterialLoader_BuildDocker.id}"
            successfulOnly = true
        }
    }

    features {
        replaceContent {
            fileRules = "+:iis-dev/%NOMAD_ENV%/%JOB_HCl%"
            pattern = "%DOCKER_IMAGE_NAME%:latest"
            replacement = "%DOCKER_IMAGE_NAME%:${MaterialLoader_BuildDocker.depParamRefs["gitHashShort"]}"
        }
    }

    dependencies {
        snapshot(MaterialLoader_BuildDocker) {
            onDependencyFailure = FailureAction.CANCEL
            onDependencyCancel = FailureAction.CANCEL
        }
    }
})

object MaterialLoader_IisContourUiHttps : GitVcsRoot({
    name = "IIS/contour-ui(https)"
    url = "https://git.warfare-tec.com/IIS/contour-ui.git"
    branch = "develop"
    branchSpec = """
        +:refs/heads/*
        +:refs/tags/*
    """.trimIndent()
    useTagsAsBranches = true
    authMethod = password {
        userName = "tc_contour"
        password = "credentialsJSON:33e32587-317c-4e6a-8230-ad7ce5143a2e"
    }
})

object MaterialLoader_IisNomad : GitVcsRoot({
    name = "IIS/nomad"
    url = "git@git.warfare-tec.com:IIS/nomad.git"
    branch = "master"
    useTagsAsBranches = true
    authMethod = uploadedKey {
        uploadedKey = "tc_contour"
    }
})
