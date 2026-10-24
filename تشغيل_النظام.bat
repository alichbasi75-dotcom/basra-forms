@echo off
chcp 65001 > nul
title نظام نماذج مكتب انتخابات البصرة 2026
echo ================================================================
echo   نظام نماذج مكتب انتخابات محافظة البصرة ومراكز التسجيل 2026
echo ================================================================
echo.
echo [1/2] جاري الانتقال لمجلد النظام...
cd /d "C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"

echo [2/2] جاري تشغيل الخادم وفتح المتصفح...
start http://localhost:3000/center

node server.js
if %errorlevel% neq 0 (
    echo.
    echo [خطأ] حدث توقف في الخادم.
    pause
)
