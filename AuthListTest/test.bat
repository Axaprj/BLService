REM CALL test_iteration.bat -SERVICE >test_service1.txt
REM pause
REM CALL test_iteration.bat -RAW >test_raw1.txt
REM CALL test_iteration.bat -SERVICE >test_service2.txt
REM pause
REM CALL test_iteration.bat  >test_cache1.txt
REM CALL test_iteration.bat -SERVICE >test_service3.txt
REM pause
REM CALL test_iteration.bat  >test_cache2.txt
REM CALL test_iteration.bat -SERVICE >test_service4.txt
REM pause
REM 
AuthListTest -DBINIT 1000
REM AuthListTest -ITERATIONS 500 -THREADS 200 -SILENT  >test_cache05k200.txt
REM AuthListTest -ITERATIONS 500 -THREADS 200 -SERVICE -SILENT   >test_service05k200.txt
REM pause

REM AuthListTest -ITERATIONS 500 -THREADS 300 -SILENT  >test_cache05k300.txt
REM AuthListTest -ITERATIONS 500 -THREADS 300 -SERVICE -SILENT   >test_service05k300.txt
REM pause

AuthListTest -ITERATIONS 500 -THREADS 400 -SILENT  >test_cache05k400.txt
AuthListTest -ITERATIONS 500 -THREADS 400 -SERVICE -SILENT   >test_service05k400.txt
pause
