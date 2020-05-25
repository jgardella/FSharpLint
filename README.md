# FSharpLint ![GitHub Actions Build Status](https://github.com/fsprojects/FSharpLint/workflows/.NET%20Core%203.1/badge.svg)]

FSharpLint is a style checking tool for F#. It points out locations where a set of rules on how F# is to be styled have been broken.
The tool is configurable via JSON and can be run from a console app, or as an MSBuild task. It also provides an interface to easily integrate the tool into other software.

The project aims to let the user know of problems through [matching user defined hints](http://fsprojects.github.io/FSharpLint/rules/FL0065.html) a la [HLint](http://community.haskell.org/~ndm/hlint/), and also by using custom rules written in F# similar to the rules in [Mascot](http://mascot.x9c.fr/manual.html) and [StyleCop](http://stylecop.codeplex.com/).

The tool in use (running as an MSBuild task with TreatWarningsAsErrors set to true):

![Example](http://i.imgur.com/D4c9g1m.png)

## Usage

FSharpLint can be used in several ways:

* [Running as dotnet tool from command line](http://fsprojects.github.io/FSharpLint/DotnetTool.html).
* [In VS Code using the Ionide-FSharp plugin](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp).
* [In other IDEs (Visual Studio, Rider) as an MSBuild Task](http://fsprojects.github.io/FSharpLint/MSBuildTask.html).
* [In other editors through FsAutoComplete Language Server](https://github.com/fsharp/FsAutoComplete)

## Documentation

The [docs](http://fsprojects.github.io/FSharpLint/) contains an overview of the tool and how to use it, including a list of the [available rules](http://fsprojects.github.io/FSharpLint/Rules.html) for linting.

## Nuget Packages

Package | Version
------- | --------
[dotnet tool](https://www.nuget.org/packages/dotnet-fsharplint/) | [![NuGet Status](http://img.shields.io/nuget/v/dotnet-fsharplint.svg?style=flat)](https://www.nuget.org/packages/dotnet-fsharplint/)
[API](https://www.nuget.org/packages/FSharpLint.Core/) | [![NuGet Status](http://img.shields.io/nuget/v/FSharpLint.Core.svg?style=flat)](https://www.nuget.org/packages/FSharpLint.Core/)

## How to build application

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. To run tests use `dotnet fake build -t Test`
5. To build documentation use `dotnet fake build -t Docs`

## How to work with documentation

1. Make sure you've installed .Net Core version defined in [global.json](global.json)
2. Run `dotnet tool restore` to install all developer tools required to build the project
3. Run `dotnet fake build` to build default target of [build script](build.fsx)
4. Build documentation to make sure everything is fine with `dotnet fake build -t Docs`
5. Go to docs folder `cd docs` and start Fornax in watch mode `dotnet fornax watch`
6. Your documentation should be now accessible on `localhost:8080` and will be regenerated on every file save

## How to release

1. Update [CHANGELOG.md](./CHANGELOG.md) by adding new entry (`## [X.Y.Z]`) and commit it.
2. Create version tag (`git tag vX.Y.Z`)
3. Run `dotnet fake build -t Pack` to create the nuget package and test/examine it locally.
4. Push the tag to the repo `git push origin vX.Y.Z` - this will start CI process that will create GitHub release and put generated NuGet packages in it
5. Upload generated packages into NuGet.org

## Licensing

The project is licensed under MIT. For more information on the license see the LICENSE file.

## Contact

Feel free to post an issue on [github](../../issues) if you have any questions, have suggestions, or have found a defect.

## Maintainer(s)

- [@duckmatt](https://github.com/duckmatt)

The default maintainer account for projects under "fsprojects" is [@fsprojectsgit](https://github.com/fsprojectsgit) - F# Community Project Incubation Space (repo management)
