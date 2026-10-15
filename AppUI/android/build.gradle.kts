allprojects {
    repositories {
        google()
        mavenCentral()
    }
}

val newBuildDir: Directory =
    rootProject.layout.buildDirectory
        .dir("../../build")
        .get()
rootProject.layout.buildDirectory.value(newBuildDir)

subprojects {
    val newSubprojectBuildDir: Directory = newBuildDir.dir(project.name)
    project.layout.buildDirectory.value(newSubprojectBuildDir)
}
// Some older plugins (e.g. media_store_plus 0.1.3) hardcode compileSdk 33 while
// pulling in androidx libraries that require compiling against API 34+. Force
// every Android library subproject to compile against at least 34 so the build
// passes. Registered via gradle.beforeProject so the afterEvaluate hook is in
// place before each subproject (and the evaluationDependsOn below) evaluates.
gradle.beforeProject {
    if (project != rootProject) {
        project.afterEvaluate {
            plugins.withId("com.android.library") {
                val android = extensions.findByType(
                    com.android.build.api.dsl.CommonExtension::class.java
                )
                if (android != null && (android.compileSdk ?: 0) < 34) {
                    android.compileSdk = 34
                }
            }
        }
    }
}

subprojects {
    project.evaluationDependsOn(":app")
}

tasks.register<Delete>("clean") {
    delete(rootProject.layout.buildDirectory)
}
