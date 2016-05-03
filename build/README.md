# Building and deploying

## How to build

'cd' into this directory, and run the 'build.cmd' command and follow the prompts ...

Alternatively, you can specify the version to avoid prompts, for example:

```
build.cmd 1.0.0
```

## How to package and nuget deploy (a reminder to myself)

```
set apiKey=<MY_NUGET_API_KEY_HERE>
set packageVersion=<VERSION_OF_THE_PACKAGE_TO_DEPLOY>
call deploy.cmd %apiKey% %packageVersion%
```
