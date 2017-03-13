echo off
echo "%*"
IF "%1"=="Debug" (
    nuget push "%2" %nugetApiKey% -source "myget.org"
)
IF "%1"=="Release" (
    nuget push "%2" %mygetApiKey% -source "nuget.org"
)