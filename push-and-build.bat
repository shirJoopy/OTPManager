@echo off
setlocal

REM Set your solution path and output path
set SOLUTION_PATH=OTPManager.sln
set BUILD_OUTPUT_PATH=Build
set ARTIFACTS_BRANCH=build-artifacts

REM Step 1: Commit and push changes to master
git checkout master
git add .
git commit -m "Committing changes to master"
git push origin master

REM Check if push was successful
if %ERRORLEVEL% neq 0 (
    echo Failed to push to master. Please resolve conflicts or other issues first.
    exit /b %ERRORLEVEL%
)

REM Step 2: Try to pull the latest changes
git pull origin master
if %ERRORLEVEL% neq 0 (
    echo Pull failed, possibly due to conflicts. Please resolve conflicts and try again.
    exit /b %ERRORLEVEL%
)

REM Step 3: Build the project
MSBuild.exe %SOLUTION_PATH% /p:Configuration=Release /p:OutputPath=%BUILD_OUTPUT_PATH%
if %ERRORLEVEL% neq 0 (
    echo Build failed. Check the build errors for more information.
    exit /b %ERRORLEVEL%
)

REM Step 4: Push build output to a different branch
git checkout --orphan %ARTIFACTS_BRANCH%
git reset --hard  # Resets the index and working tree. Any changes to tracked files in the working tree since <commit> are discarded.

REM Copy build output to the root of the working directory and commit
xcopy /E /I %BUILD_OUTPUT_PATH% .
git add .
git commit -m "Updated build artifacts on %date%"
git push -u origin %ARTIFACTS_BRANCH% --force

REM Cleanup and return to master
git checkout master
git branch -D %ARTIFACTS_BRANCH%

echo Build and push completed successfully.
endlocal
