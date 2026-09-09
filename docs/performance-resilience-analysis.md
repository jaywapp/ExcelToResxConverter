# 성능·회귀 테스트·안정성 분석

orchestrator: Codex

사용자 요청: 질문 없이 기존 동작을 보존하는 성능 개선, 핵심·실패·빈 값·입력/권한 오류 테스트, 실패 경계 방어를 수행한다.
저장소: ExcelToResxConverter. 루트 AGENTS.md, CLAUDE.md, README와 소스 구조를 확인했다. 초기 Git 상태는 상위 오케스트레이터가 확인한 깨끗한 codex/workspace-environment-20260904 브랜치다.
가정: 기존 계약과 확인 가능한 소스만 기준으로 한다. 외부 서비스, 실제 자격 증명, 배포 및 UI 디자인 변경은 제외한다. 질문하지 말고 수정하라는 명시적 승인을 적용한다.
완료 기준: 근거 있는 변경과 경계 회귀 검증. 실행 환경이 없는 항목 및 변경 근거가 없는 항목은 별도로 기록한다.

확인 결과: ResourceUnit의 빈 번역 셀(null)을 빈 문자열로 기록해 XElement.Value 예외를 방지한다. 유효 문자열과 XML escaping은 유지한다.
검증 범위 및 한계: 원본 WPF 빌드는 기존 ExcelDataReader/Ookii/Prism/ReactiveUI 참조 DLL 누락으로 실패. 새 테스트는 ResourceUnit/ResxBuilder 실제 소스를 .NET 9에서 검증. 실제 Excel reader 및 GUI 전체는 미검증.
