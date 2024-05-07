@echo off
setlocal

REM Define paths and branch names
set SOLUTION_PATH=OTPManager.sln
set BUILD_OUTPUT_PATH=Build
set ARTIFACTS_BRANCH=build-artifacts

REM Ensure the script is committed to avoid being deleted
git add push-and-build.bat
git commit -m "Add/update batch script for building and deployment."

REM Checkout master and pull latest changes
git checkout master
git pull origin master

REM Build the solution
MSBuild.exe %SOLUTION_PATH% /p:Configuration=Release /p:OutputPath=%BUILD_OUTPUT_PATH%
if %ERRORLEVEL% neq 0 (
    echo Build failed. Check the build errors for more information.
    exit /b %ERRORLEVEL%
)

REM Create or switch to a clean state of the artifacts branch
git checkout -B %ARTIFACTS_BRANCH%
git rm -rf .
git clean -fdx

REM Copy build artifacts from the build directory
xcopy /E /I %BUILD_OUTPUT_PATH% .

REM Add and commit the new artifacts
git add -A
git commit -m "Updated build artifacts on %date%"

REM Push the artifacts to the remote repository
git push origin %ARTIFACTS_BRANCH% --force

REM Cleanup and return to master
git checkout master

echo Build and push completed successfully.
endlocal
