# 빌드 및 실행 검증 작업

orchestrator: Codex

| 작업 | owner | model | effort | depends_on | parallel_group | files | verification | status |
|---|---|---|---|---|---|---|---|---|
| 원인 조사 및 복원 | Codex | gpt-6-astra | high | 없음 | excel | csproj 읽기 | Release 빌드 | completed |
| 원본 어셈블리 통합 검사 | Codex | gpt-6-astra | high | 복원 | excel | tests/IntegrationTests | 임시 XLSX 및 WPF | completed |
| 검증 및 문서 | Codex | gpt-6-astra | high | 통합 검사 | excel | docs, README | Debug/Release, 회귀 | completed |

복원→테스트 구현→검증 의존성으로 순차 수행한다. 루트 Codex가 다른 저장소를 병렬 조정한다. commit/push/PR/merge 없음.

## 실행 결과

- Visual Studio 2022 Community MSBuild, 원본 솔루션 Release /restore /t:Build: 성공.
- IntegrationTests.csproj /restore /t:Rebuild /p:Configuration=Release: 원본 프로젝트와 검사 프로젝트 빌드 성공, 실행 21개 통과.
- 동일 Debug 재빌드: 성공, 실행 21개 통과.
- dotnet run --project tests/RegressionTests/RegressionTests.csproj: 기존 8개 통과. 기존 ResxBuilder의 nullable 경고 CS8602/CS8603/CS8618은 남는다.
- git diff --check: 통과.

통합 검사는 원본 .NET Framework 어셈블리를 직접 로드한다. 임시 XLSX 2개 시트와 빈 셀, 한글, 특수문자, 빈 시트, 없는 입력, 잘못된 형식, 숫자 key의 기존 거부 계약, 입력 파일 정상/오류 종료 후 해제, 잠긴 입력·출력, 읽기 전용 출력 접근 거부, 없는 출력 디렉터리를 검사한다. 원본 template.xml로 양 언어 RESX를 저장·재읽고 WPF 창 생성 및 콘텐츠 레이아웃을 실제 실행한다. 테스트 임시 폴더는 finally에서 정리한다.

한계: WPF 창을 화면에 표시하거나 네이티브 파일 선택 대화상자 및 성공/실패 MessageBox를 클릭하지 않는다. ConvertCommand 전체 UI 흐름, 실제 사용자 파일, ACL 기반 접근 거부와 모든 Excel 형식은 미검증이다. 읽기 전용 임시 출력 파일로 UnauthorizedAccessException을 검증했으며 사용자 ACL을 변경하지 않는다.

원본 실패는 패키지 선언 누락이 아닌 복원/실행 환경 문제였다. 일반 sandbox의 NuGet 연결 및 PowerShell 파일 쓰기 접근 실패 이후 승인된 권한 확대 빌드와 apply_patch 편집으로 완료했다. 제품 코드·csproj·패키지 버전은 변경하지 않았다.
