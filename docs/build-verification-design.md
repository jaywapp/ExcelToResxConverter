# 빌드 및 실행 검증 설계

Visual Studio MSBuild /restore로 기존 패키지를 복원한다. 런타임·패키지 버전 변경 또는 제품 리팩터링을 하지 않는다.

별도 .NET Framework 콘솔 테스트가 원본 WPF 프로젝트를 ProjectReference로 참조한다. ZipArchive로 임시 XLSX fixture를 생성하며 추가 NuGet 패키지를 도입하지 않는다. 실제 ExcelReader, ResxBuilder, MainWindow를 실행한다.

검증 범위: 다중 시트, 빈 번역, XML 특수문자, 빈 시트, 없는 파일, 손상 입력, 독점 잠금, 숫자 셀 실패, 파일 해제, 실제 RESX 저장, WPF 및 ViewModel 초기화. 네이티브 파일 선택 대화상자와 MessageBox 클릭은 범위 밖으로 명시한다.

최종 검증: 임시 출력 파일에만 읽기 전용 속성을 설정하고 쓰기 거부를 확인한 뒤 finally로 속성을 복구한다. WPF 창은 표시하지 않고 실제 콘텐츠의 Measure/Arrange를 실행한다. 두 빌드 구성에서 21개 통합 검사를 통과했다.
