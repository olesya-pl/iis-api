import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.dockerSupport
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.script
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

    subProject(Api)
}


object Api : Project({
    name = "API"

    vcsRoot(Api_IisContourUiHttps)

    buildType(Api_BuildDocker)
})

object Api_BuildDocker : BuildType({
    name = "Build Docker"

    params {
        param("env.CI_BUILD_VERSION", "%teamcity.build.branch%.%system.build.number%")
    }

    vcs {
        root(Api_IisContourUiHttps)
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
