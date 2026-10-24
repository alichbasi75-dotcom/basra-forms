import os
import shutil
import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

src_dir = r"C:\Users\ali\Desktop\نـــمـــاذج 2026"
dst_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\uploads"

# Clean uploads directory
for f in os.listdir(dst_dir):
    try:
        os.remove(os.path.join(dst_dir, f))
    except Exception:
        pass

# Exactly the 22 valid files from Desktop\نماذج 2026
official_files_list = [
    # 1. الإجازات والدوام والمباشرات
    {
        "origFile": "نموذج طلب اجازة خاصه .docx",
        "title": "طلب إجازة خاصة (من 1 إلى 3 أيام)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "docx",
        "desc": "ترسل لوحدة شؤون المراكز - خاصة بطلب إجازة اعتيادية قصيرة من يوم إلى 3 أيام.",
        "fillableId": "tpl_leave_short"
    },
    {
        "origFile": "نموذج اجازة اعتيادية من 4 الى 5 ايام ترسل لمكتب انتخابات البصرة بالبريد .doc",
        "title": "طلب إجازة اعتيادية (من 4 إلى 5 أيام)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "doc",
        "desc": "ترسل لمكتب انتخابات البصرة بالبريد الرسمي - للمدد من 4 إلى 5 أيام.",
        "fillableId": "tpl_leave_medium"
    },
    {
        "origFile": "نموذج اجازة من 6 ايام و اكثر  ترسل للمكتب الوطني وتؤخذ باليد لمكتب انتخابات البصرة .doc",
        "title": "طلب إجازة طويلة (من 6 أيام فأكثر)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "doc",
        "desc": "ترفع للمكتب الوطني وتسلّم باليد لمكتب المحافظة مع استمارة الإجازة الطويلة.",
        "fillableId": "tpl_leave_long"
    },
    {
        "origFile": "نموذج استمارة الاجازة الطويلة.pdf",
        "title": "استمارة الإجازة الطويلة (نسخة PDF)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "pdf",
        "desc": "الاستمارة المعتمدة التي ترفق إجبارياً مع طلبات الإجازة من 6 أيام فأكثر.",
        "fillableId": None
    },
    {
        "origFile": "نموذج استمارة الاجازة الطويلة.jpg",
        "title": "استمارة الإجازة الطويلة (نسخة صورة JPG)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "image",
        "desc": "نسخة صورة عالية الدقة لاستمارة الإجازة الطويلة للطباعة والاستخدام الفوري.",
        "fillableId": None
    },
    {
        "origFile": "نموذج الاستمارة للاجازة الطويلة ترفق مع الطلب.pdf",
        "title": "استمارة الإجازة الطويلة المرفقة مع الطلب (نسخة ثانية)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "pdf",
        "desc": "صيغة مجهزة للطباعة لترفق مع طلب الإجازة الطويلة لمكتب المحافظة.",
        "fillableId": None
    },
    {
        "origFile": "نموذج انفكاك.docx",
        "title": "استمارة انفكاك موظف للتمتع بالإجازة",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "docx",
        "desc": "كتاب رسمي لانفكاك الموظف استناداً للأمر الإداري الصادر بمنحه الإجازة.",
        "fillableId": "tpl_infikak"
    },
    {
        "origFile": "نموذج المباشرة.doc",
        "title": "استمارة مباشرة موظف بعد الإجازة",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "doc",
        "desc": "إشعار رسمي بمباشرة الموظف بالدوام الفعلي في مركز التسجيل بعد انتهاء الإجازة.",
        "fillableId": "tpl_mubashara"
    },
    {
        "origFile": "فورمة براءة ذمة.pdf",
        "title": "استمارة براءة ذمة موظف",
        "category": "leaves",
        "categoryName": "الإجازات والدوام والمباشرات",
        "type": "pdf",
        "desc": "براءة ذمة موظفي المراكز عند النقل أو الانفكاك أو إخلاء العهدة.",
        "fillableId": None
    },

    # 2. الأجهزة والصيانة
    {
        "origFile": "نموذج الفحص و المعالجة.doc",
        "title": "استمارة الفحص والمعالجة الفنية للأجهزة",
        "category": "it",
        "categoryName": "الأجهزة والصيانة",
        "type": "doc",
        "desc": "تقرير فحص وصيانة أجهزة التحقق البايومتري والتسجيل المرسلة للصيانة.",
        "fillableId": "tpl_maintenance"
    },

    # 3. البطاقات المفقودة والإخبار
    {
        "origFile": "نموذج الاخبار.pdf",
        "title": "نموذج الإخبار عن فقدان بطاقة ناخب",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "pdf",
        "desc": "إخبار رسمي عن فقدان بطاقة الناخب لاستكمال إجراءات المحكمة وإصدار البديل.",
        "fillableId": None
    },
    {
        "origFile": "نموذج التعهد.pdf",
        "title": "نموذج تعهد خطي لفقدان البطاقة",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "pdf",
        "desc": "تعهد خطي يوقعه المواطن الناخب بشأن فقدان أو تسليم بطاقة الناخب.",
        "fillableId": None
    },
    {
        "origFile": "نموذج السجل الخاص بالبطاقات المفقودة.xlsx",
        "title": "سجل حصر وتوثيق البطاقات المفقودة (Excel)",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "xlsx",
        "desc": "جدول إكسل منظم لتدوين بيانات المواطنين فاقدي بطاقاتهم وأرقام بلاغاتهم.",
        "fillableId": None
    },
    {
        "origFile": "فورمة جدول بلاغ المحكمه (المفقودة.xlsx",
        "title": "جدول بلاغات المحكمة للبطاقات المفقودة (Excel)",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "xlsx",
        "desc": "قائمة بلاغات المحكمة وتواريخها الخاصة بقرارات البطاقات البايومترية.",
        "fillableId": None
    },

    # 4. التقارير الإدارية وسجلات الحضور
    {
        "origFile": "استمارة رقم 3 اليومية.pdf",
        "title": "استمارة رقم (3) للموقف والتقرير اليومي",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "الاستمارة اليومية المعتمدة لرفع موقف التسجيل والتوزيع بالمركز.",
        "fillableId": None
    },
    {
        "origFile": "مطالعة نسب انجاز الجوالة.pdf",
        "title": "مطالعة نسب إنجاز الفرق الجوالة",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "كتاب مطالعة أسبوعي/شهري لحصر نسب إنجاز الفرق الميدانية وتحديث البطاقات.",
        "fillableId": None
    },
    {
        "origFile": "استمارة الاستقطاع المباشر.pdf",
        "title": "استمارة الاستقطاع المباشر للموظفين",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "استمارة المحاسبة والشعبة المالية الخاصة بالاستقطاعات القانونية والرواتب.",
        "fillableId": None
    },
    {
        "origFile": "نموذج سجل حضور وانصراف 2928 2026.xlsx",
        "title": "سجل الحضور والانصراف اليومي للمركز 2026 (Excel)",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "xlsx",
        "desc": "شيت إكسل جاهز لتدوين أوقات حضور وانصراف وتواقيع موظفي مركز التسجيل.",
        "fillableId": None
    },
    {
        "origFile": "تسلسل الموظفين يكتب على النماذج.xlsx",
        "title": "جدول تسلسل وعناوين موظفي المركز (Excel)",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "xlsx",
        "desc": "قائمة تسلسل كودات وأسماء الموظفين المثبتة على المخاطبات الإدارية.",
        "fillableId": None
    },

    # 5. نماذج استرشادية توضيحية
    {
        "origFile": "2926  اجازة خاصة.pdf",
        "title": "نموذج استرشادي: طلب إجازة خاصة معبأ ومكتمل",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة نموذجية توضح للموظف ومسؤول المركز طريقة ملء وتوقيع الإجازة الخاصة.",
        "fillableId": None
    },
    {
        "origFile": "2926  اجازة الاعتيادبة  من 4 الى5.pdf",
        "title": "نموذج استرشادي: إجازة اعتيادية (4 إلى 5 أيام) معبأ",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة معبأة نموذجية توضح أسلوب تدوين التواريخ والموظف البديل.",
        "fillableId": None
    },
    {
        "origFile": "2926  اجازة اطويلة  من 6 الى اخرى.pdf",
        "title": "نموذج استرشادي: إجازة طويلة (6 أيام فأكثر) معبأ",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة معبأة نموذجية توضح مرفقات طلب الإجازة الطويلة المرفوعة للإدارة.",
        "fillableId": None
    }
]

forms_output = []
for idx, item in enumerate(official_files_list, 1):
    src_file_path = os.path.join(src_dir, item["origFile"])
    dst_file_name = f"form_{idx:02d}_{item['origFile']}"
    dst_file_path = os.path.join(dst_dir, dst_file_name)

    if not os.path.exists(src_file_path):
        print(f"ERROR: {src_file_path} not found!")
        continue

    shutil.copy2(src_file_path, dst_file_path)
    sz = os.path.getsize(dst_file_path)
    sz_str = f"{round(sz/1024, 1)} KB" if sz < 1024*1024 else f"{round(sz/(1024*1024), 1)} MB"

    # Check for view preview file
    base_no_ext = os.path.splitext(item["origFile"])[0]
    preview_file_candidate = os.path.join(src_dir, f"{base_no_ext}_view.pdf")
    preview_file_name = None

    if os.path.exists(preview_file_candidate):
        dst_preview_name = f"preview_{idx:02d}.pdf"
        dst_preview_path = os.path.join(dst_dir, dst_preview_name)
        shutil.copy2(preview_file_candidate, dst_preview_path)
        preview_file_name = dst_preview_name
    elif item["type"] in ["pdf", "image"]:
        preview_file_name = dst_file_name

    forms_output.append({
        "id": f"form_{idx:02d}",
        "title": item["title"],
        "category": item["category"],
        "categoryName": item["categoryName"],
        "fileType": item["type"],
        "fileName": dst_file_name,
        "originalName": item["origFile"],
        "previewFile": preview_file_name,
        "fileSize": sz_str,
        "uploadDate": "2026-10-21",
        "updatedDate": "2026-10-21",
        "version": "2026",
        "description": item["desc"],
        "fillableTemplateId": item["fillableId"],
        "downloads": 0
    })
    print(f"[{idx:02d}/22] ✓ {item['title']} -> {dst_file_name} (Preview: {preview_file_name})")

out_json_path = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\data\forms.json"
with open(out_json_path, "w", encoding="utf-8") as f:
    json.dump(forms_output, f, ensure_ascii=False, indent=2)

print(f"\nSuccessfully verified all {len(forms_output)} official forms!")
