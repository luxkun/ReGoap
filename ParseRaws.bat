REM Helper bat file to create ps files with Graphiz's dot
REM Graphiz must be installed and must be in Windows's PATH, alternatively just put your 'dot.exe' directory in the following var

SET "dotPath="
REM Change this to the working directory, in Unity, if you place "ReGoap" inside Assets/, it will be '../../PlanDebugger'
SET "workingPath=../../PlanDebugger/"
SET "rawPath=Raws"
SET "resultsPath=PDFs"
SET "outputType=pdf"
SET "rawSearch=%rawPath%/*"

SET "currentDir=%cd%"

CD %workingPath%
ECHO "Changing working path to %workingPath%. Parsing files in %rawPath%. Saving in %resultsPath% with file type '%outputType%'."

MKDIR %resultsPath%
FOR %%I IN (%rawSearch%) DO %dotPath%dot -T%outputType% %rawPath%/%%I -o %resultsPath%/%%I.pdf

explorer %cd%\%resultsPath%

CD %currentDir%