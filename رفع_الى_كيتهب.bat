@echo off
chcp 65001 >nul
title رفع المنظومة إلى GitHub Pages
color 0b

echo ======================================================================
echo       بوابة استمارات مراكز التسجيل - الرفع إلى GitHub Pages
echo ======================================================================
echo.
echo حسابك على GitHub هو: alichbasi75-dotcom
echo.
echo يرجى التأكد من إنشاء مستودع (Repository) جديد على حسابك في GitHub
echo ويفضل تسميته: basra-forms
echo وجعله Public (عام) وبدون إضافة README أو .gitignore
echo.
set /p REPO_NAME="ادخل اسم المستودع (اضغط Enter للاسم الافتراضي basra-forms): "
if "%REPO_NAME%"=="" set REPO_NAME=basra-forms

echo.
echo جاري ربط المستودع: https://github.com/alichbasi75-dotcom/%REPO_NAME%.git ...

git remote remove origin 2>nul
git remote add origin https://github.com/alichbasi75-dotcom/%REPO_NAME%.git
git branch -M main

echo.
echo جاري رفع الملفات إلى GitHub... (قد تظهر نافذة لتسجيل الدخول إلى GitHub)
git push -u origin main

if %errorlevel% equ 0 (
    echo.
    echo ======================================================================
    echo   تم الرفع بنجاح وبشكل كامل!
    echo ======================================================================
    echo.
    echo الخطوة المتبقية في GitHub لتفعيل الرابط:
    echo 1. ادخل إلى إعدادات المستودع (Settings) -> Pages
    echo 2. تحت (Build and deployment) اختر: Deploy from a branch
    echo 3. اختر Branch: main والمجلد: / (root) ثم اضغط Save
    echo.
    echo رابط المنظومة للمراكز سيكون:
    echo https://alichbasi75-dotcom.github.io/%REPO_NAME%/
    echo ======================================================================
) else (
    echo.
    echo حدث خطأ أثناء الرفع، تأكد من اتصال الإنترنت وصلاحيات الدخول إلى GitHub.
)

echo.
pause
