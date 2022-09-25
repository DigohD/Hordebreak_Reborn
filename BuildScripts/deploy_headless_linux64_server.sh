echo Cleaning up Build directory
rm -rf ../../ProjectFNZ/Builds

echo Starting Build Process
'/f/Unity/2019.2.12f1/Editor/Unity.exe' -quit -batchmode -projectPath ../../ProjectFNZ -executeMethod BuildScript.BuildHeadlessLinux64Server
echo Ended Build Process