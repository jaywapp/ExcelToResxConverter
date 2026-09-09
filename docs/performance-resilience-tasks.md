# 성능·안정성 실행 작업

orchestrator: Codex

| 작업 | owner | model | effort | depends_on | parallel_group | files | verification | status |
|---|---|---|---|---|---|---|---|---|
| 저장소 조사 | Codex | gpt-6-astra | high | 없음 | dotnet-repos | 규칙·README·소스 | 소스 검토 | completed |
| 확인된 개선 및 회귀 | Codex | gpt-6-astra | high | 저장소 조사 | dotnet-repos | tests/RegressionTests/Program.cs 및 관련 소스 | 아래 결과 | completed |
| 전체 기능 통합 검증 | Codex | gpt-6-astra | high | 확인된 개선 및 회귀 | dotnet-repos | 저장소 전체 | 아래 한계 | not_completed |

ResourceUnit의 빈 번역 셀(null)을 빈 문자열로 기록해 XElement.Value 예외를 방지한다. 유효 문자열과 XML escaping은 유지한다.

검증: dotnet run --project tests/RegressionTests/RegressionTests.csproj: 8개 검사 통과.

한계: 원본 WPF 빌드는 기존 ExcelDataReader/Ookii/Prism/ReactiveUI 참조 DLL 누락으로 실패. 새 테스트는 ResourceUnit/ResxBuilder 실제 소스를 .NET 9에서 검증. 실제 Excel reader 및 GUI 전체는 미검증.

위 완료 표시는 확인된 변경과 회귀 범위에 한정한다. 모든 기능·모든 실패 상황의 테스트 작성을 완료했다는 의미가 아니다. 그룹 간에는 상위 Codex 세션과 병렬 진행했고 그룹 내부는 조사→변경→검증 의존성으로 순차 진행했다. commit/push/배포 없음.
