# ExcelToResxConverter
Converting Utility from excel file to resx file.

## 빌드 및 검증

Windows, Visual Studio MSBuild와 .NET Framework 4.7.2 개발자 팩을 사용한다. 기존 NuGet 패키지를 복원하려면 최초 빌드에 네트워크 연결이 필요하다. PowerShell에서 저장소 루트를 기준으로 실행한다.

```powershell
$msbuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
& $msbuildPath ExcelToResxConverter.sln /restore /t:Build /p:Configuration=Release
& $msbuildPath tests\IntegrationTests\IntegrationTests.csproj /restore /t:Rebuild /p:Configuration=Release
& .\tests\IntegrationTests\bin\Release\IntegrationTests.exe
dotnet run --project tests\RegressionTests\RegressionTests.csproj
```

설치 에디션이 다르면 MSBuild 경로를 조정한다. 기존 회귀 실행에는 .NET 9 SDK가 필요하다. 통합 검사는 원본 .NET Framework 어셈블리와 임시 XLSX를 사용하며 실제 사용자 파일에 접근하지 않는다. Release를 Debug로 바꾸면 Debug 구성도 검사할 수 있다. 상세 결과와 미검증 범위는 [빌드 검증 문서](docs/build-verification-tasks.md)에 기록했다.
