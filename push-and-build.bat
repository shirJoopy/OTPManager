@echo off
setlocal

REM Define the solution and build output paths
set SOLUTION_PATH=OTPManager.sln
set BUILD_OUTPUT_PATH=Build
set ARTIFACTS_BRANCH=build-artifacts
set BATCH_FILE_NAME=push-and-build.bat

REM Navigate to the project directory
cd path\to\your\project

REM Checkout master and ensure it's up to date
git checkout master
git pull origin master

REM Add all changes except the batch file, commit them, and push
git add --all
git commit -m "Commit local changes before build"
git push origin master

REM Build the project
MSBuild.exe "%SOLUTION_PATH%" /p:Configuration=Release /p:OutputPath="%BUILD_OUTPUT_PATH%"
if %ERRORLEVEL% neq 0 (
    echo Build failed. Check the build errors for more information.
    exit /b %ERRORLEVEL%
)

REM Switch to a clean state of the artifacts branch
git checkout --orphan %ARTIFACTS_BRANCH%
git reset --hard
git clean -fdx

REM Remove all files from the old working tree, except the batch file
for /F "tokens=*" %%i in ('dir /b /a-d ^| find /v "%BATCH_FILE_NAME%"') do del "%%i"
for /D %%d in (*) do if not "%%d"=="%BUILD_OUTPUT_PATH%" rd /s /q "%%d"

REM Copy build outputs to the current directory
xcopy /Y /I /E "%BUILD_OUTPUT_PATH%\*" .

REM Add and commit the new artifacts
git add .
git commit -m "Updated build artifacts on %date%"

REM Push the updates to remote
git push origin %ARTIFACTS_BRANCH% --force

REM Cleanup by returning to master and removing temporary changes
git checkout master
git branch -D %ARTIFACTS_BRANCH%

echo Build and push to artifacts branch completed successfully.
endlocal
