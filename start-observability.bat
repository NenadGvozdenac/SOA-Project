@echo off
echo 🚀 Starting SOA Project with full Observability Stack...

REM Build and start all services
docker-compose up -d

echo ⏳ Waiting for services to start...
timeout /t 30 /nobreak > nul

echo ✅ Services started! Access points:
echo.
echo 📱 Application:
echo   - Frontend: http://localhost:5173
echo   - Stakeholders API: http://localhost:8080
echo   - Tours API: http://localhost:8082
echo   - Blogs API: http://localhost:8081
echo   - Followings API: http://localhost:9090
echo.
echo 📊 Observability Tools:
echo   - Jaeger (Tracing): http://localhost:16686
echo   - Kibana (Logs): http://localhost:5601
echo   - Grafana (Metrics): http://localhost:3000 (admin/admin)
echo   - Prometheus: http://localhost:9090
echo.
echo 🔧 Generate some traffic for testing:
echo   curl http://localhost:8080/api/health
echo   curl http://localhost:8081/api/health
echo   curl http://localhost:8082/api/health
echo   curl http://localhost:9090/api/health
echo.
echo 🎉 All systems ready!
pause
