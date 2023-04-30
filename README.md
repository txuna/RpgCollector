### 컴투스 서버캠퍼스 1기 
김수창


### 2차 피드백  
2차 피드백  
[해결]1. 중복로그인 체크하지 말고 -> 새로운값으로 토큰 발급하고 갱신  + TTL 추가   
[해결]2. 컨트롤러마다 Request, Response 사용하기    
[해결]3. Response에 true false + message가 아닌 Error코드 정의해서 전송   
[해결]4. Document폴더만들어서 MD파일들 넣기    
[해결]5. 패스워드 유효성 검사는 로그인 컨트롤러에서    
[해결]6. Json으로 반환안해도 됨 -> 알아서 모델 매칭 해주는듯    
[해결]7. Redis에 User저장할때 해쉬로 X -> 간단하게 username - authToken - ttl 설정하기    
[해결]8. Insert한 row의 auto_increase한 값 가지고 오는법 찾기   
[해결]9. [FromBody] 에러뜨는 이유 알기    
-> [ApiController] : 모델 바인딩     
[해결]10. ModelState.Isvalid 없어도 되는지 확인    
[해결]11 계정은 만들어졌지만 CreatePlayer 실패했을 때 Undo기능 추가하기   
[해결]12. 에러 로깅   
[해결]13. SetupSaltAndHash 나누기    
[해결]14. Database 커넥터 각 파일에 그냥 넣기   
[해결]15. 토큰 유니코드 문제 바이트에서 스트링으로 해결하기   
[보류]16. Register시 try - catch 잘생각해보기 - 유저있는지 없는지 확인하는데 Error발산 좀 그럼    
-> Insert시 에러발생 말고 다른 메소드 있는지 찾아보기   
-> INSERT IGNORE INTO 를 사용하려고 만들어진 rawSQL을 수정해서 다시 컴파일하고 실행하려는 방법을 찾던 도중 실패   