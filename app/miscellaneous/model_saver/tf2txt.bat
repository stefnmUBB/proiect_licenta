@echo off

set inf=%~1
echo %inf%
set outf=%inf%.txt
echo %outf%

echo Converting
C:\Users\Stefan\miniconda3\envs\kaggle\python.exe D:\Public\model_saver\main.py %inf% %outf%
echo Done.
pause