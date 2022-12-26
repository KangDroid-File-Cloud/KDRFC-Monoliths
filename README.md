# KangDroid File-Cloud
![.NET 6.0](https://img.shields.io/badge/dotnet-6.x-512BD4?logo=.NET)
![Open Telemetry .NET](https://img.shields.io/badge/OpenTelemetry.NET-1.0.0-000000?logo=OpenTelemetry)
![Azure SQL Edge](https://img.shields.io/badge/AzureSQL-Edge-000000?logo=Microsoft%2dAzure)
![Github Action(Workflow)](https://img.shields.io/github/actions/workflow/status/KangDroid-File-Cloud/KDRFC-Monoliths/dotnet_test_deploy.yml?logo=GitHub%2dActions) 
![Docker Image Size](https://img.shields.io/docker/image-size/kangdroid/kdrfc?logo=Docker)
![Hardware Supported Architecture](https://img.shields.io/badge/Suppported%20Architecture-X86%2C%20ARM64-lightgrey)
![Software Supported OS](https://img.shields.io/badge/Supported%20OS-macOS%2C%20Linux-informational)
<br>

KangDroid-File-Cloud(a.k.a KDRFC in shorthand) is a `Personal, technical project` to experiment with other backend technology that I've never used in the company.<br>
This project first aims for learning, adapting new tech such as IaaC(Terraform/K8s), backend(asp.net) architecture, monitoring strategy, etc. <br>

As project name describes, this project also aims to make somewhat blob-storage just like Azure Blob Storage, AWS S3, Google Bucket.

## Features
> More comming through!

## Developing & Running Projects
### Pre-requisites
In order to develop/run KDRFC on your machine, at least theses components are needed:
- .NET 6.x (Only needed when you have to run this project from code, i.e local development.)
  - Maybe your favorite editor, such as JetBrains Rider, Microsoft Visual Studio. 
- Docker + Docker-Compose

### Run project within Docker-Compose(Container)
This section explains about running everything within docker-compose, Redis, Azure SQL Edge, Prometheus, KDRFC Web Application itself. <br>
Navigate to `$REPO_ROOT/runner` and you can see docker-compose file with prometheus scrape configuration. `$REPO_ROOT/runner` is docker context path, so in order to properly initiate docker-compose, you need to invoke `docker-compose up` from `$REPO_ROOT/runner`.

```shell
$ cd $REPO_ROOT/runner
$ docker-compose up --build # This will run all application infrastructure + Build KDRFC Container Image straight away.
```

### Run project directly from code
Running project directly from code requires two big steps
1. Initialize Application Infrastructure Dependency, such as Redis, Azure SQL.
2. Run Project from code.

To initialize application infrastructure dependency ONLY, you can invoke `docker-compose` command with `docker-compose.yml` in `$REPO_ROOT`.<br>
```shell
$ cd $REPO_ROOT
$ docker-compose up -d # Note: -d is optional field.
```

### Two Docker-Compose YAML Files. Key Differences
This project has two different `docker-compose.yml` file; - one for running application infrastructure dependency such as Redis, Azure SQL Edge - another one for running whole project with infrastructure.<br>
- `$REPO_ROOT/docker-compose.yml` -> Only contains application infrastructure dependency, with port exposure
  - Contains Redis, Azure SQL Edge
- `$REPO_ROOT/runner/docker-compose.yml` -> Contains application infrastructure with KDRFC Web Application as well(All In One.)
  - Contains Redis, Azure SQL Edge, Prometheus, KDRFC Web Application(Web App is being built every time when invoking `docker-compose` with `--build` flags.)
  - Only Prometheus and KDRFC Web Application exposes it's port.

### Testing Projects
This project consists of two tests, which is  `Unit test` + `Integration Test`.<br>
I use xUnit for main test sdk, Moq for Unit Test Mocking, `Microsoft.AspNetCore.Mvc.Testing` for Integration End to End Testing. For Integration testing, I use `TestContainer` for automatically setup application's infrastructure dependency.

Invoke as follows:
```shell
$ cd $REPO_ROOT
$ dotnet test # This will test all test case under solution, including E2E Test.
```
