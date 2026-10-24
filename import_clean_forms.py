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

forms_catalog = [
    # 1. الإجازات والدوام
    {
        "file": "نموذج طلب اجازة خاصه .docx",
        "title": "طلب إجازة خاصة (من 1 إلى 3 أيام)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "docx",
        "desc": "ترسل لوحدة شؤون المراكز - خاصة بطلب إجازة اعتيادية قصيرة من يوم إلى 3 أيام.",
        "fillableId": "tpl_leave_short"
    },
    {
        "file": "نموذج اجازة اعتيادية من 4 الى 5 ايام ترسل لمكتب انتخابات البصرة بالبريد .doc",
        "title": "طلب إجازة اعتيادية (من 4 إلى 5 أيام)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "doc",
        "desc": "ترسل لمكتب انتخابات البصرة بالبريد الرسمي - للمدد من 4 إلى 5 أيام.",
        "fillableId": "tpl_leave_medium"
    },
    {
        "file": "نموذج اجازة من 6 ايام و اكثر  ترسل للمكتب الوطني وتؤخذ باليد لمكتب انتخابات البصرة .doc",
        "title": "طلب إجازة طويلة (من 6 أيام فأكثر)",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "doc",
        "desc": "ترفع للمكتب الوطني وتسلّم باليد لمكتب المحافظة مع استمارة الإجازة الطويلة.",
        "fillableId": "tpl_leave_long"
    },
    {
        "file": "نموذج استمارة الاجازة الطويلة.pdf",
        "title": "استمارة الإجازة الطويلة المرفقة مع الطلب",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "pdf",
        "desc": "الاستمارة المعتمدة التي ترفق إجبارياً مع طلبات الإجازة من 6 أيام فأكثر.",
        "fillableId": None
    },
    {
        "file": "نموذج انفكاك.docx",
        "title": "استمارة انفكاك موظف للتمتع بالإجازة",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "docx",
        "desc": "كتاب رسمي لانفكاك الموظف استناداً للأمر الإداري الصادر بمنحه الإجازة.",
        "fillableId": "tpl_infikak"
    },
    {
        "file": "نموذج المباشرة.doc",
        "title": "استمارة مباشرة موظف بعد الإجازة",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "doc",
        "desc": "إشعار رسمي بمباشرة الموظف بالدوام الفعلي في مركز التسجيل بعد انتهاء الإجازة.",
        "fillableId": "tpl_mubashara"
    },
    {
        "file": "فورمة براءة ذمة.pdf",
        "title": "استمارة براءة ذمة موظف",
        "category": "leaves",
        "categoryName": "الإجازات والدوام",
        "type": "pdf",
        "desc": "براءة ذمة موظفي المراكز عند النقل أو الانفكاك أو إخلاء العهدة.",
        "fillableId": None
    },

    # 2. الأجهزة والصيانة
    {
        "file": "نموذج الفحص و المعالجة.doc",
        "title": "استمارة الفحص والمعالجة الفنية للأجهزة",
        "category": "it",
        "categoryName": "الأجهزة والصيانة",
        "type": "doc",
        "desc": "تقرير فحص وصيانة أجهزة التحقق البايومتري والتسجيل المرسلة للصيانة.",
        "fillableId": "tpl_maintenance"
    },

    # 3. البطاقات المفقودة والإخبار
    {
        "file": "نموذج الاخبار.pdf",
        "title": "نموذج الإخبار عن فقدان بطاقة ناخب",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "pdf",
        "desc": "إخبار رسمي عن فقدان بطاقة الناخب لاستكمال إجراءات المحكمة وإصدار البديل.",
        "fillableId": None
    },
    {
        "file": "نموذج التعهد.pdf",
        "title": "نموذج تعهد خطي لفقدان البطاقة",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "pdf",
        "desc": "تعهد خطي يوقعه المواطن الناخب بشأن فقدان أو تسليم بطاقة الناخب.",
        "fillableId": None
    },
    {
        "file": "نموذج السجل الخاص بالبطاقات المفقودة.xlsx",
        "title": "سجل حصر وتوثيق البطاقات المفقودة (Excel)",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "xlsx",
        "desc": "جدول إكسل منظم لتدوين بيانات المواطنين فاقدي بطاقاتهم وأرقام بلاغاتهم.",
        "fillableId": None
    },
    {
        "file": "فورمة جدول بلاغ المحكمه (المفقودة.xlsx",
        "title": "جدول بلاغات المحكمة للبطاقات المفقودة (Excel)",
        "category": "cards",
        "categoryName": "البطاقات المفقودة والإخبار",
        "type": "xlsx",
        "desc": "قائمة بلاغات المحكمة وتواريخها الخاصة بقرارات البطاقات البايومترية.",
        "fillableId": None
    },

    # 4. التقارير الإدارية وسجلات الحضور
    {
        "file": "استمارة رقم 3 اليومية.pdf",
        "title": "استمارة رقم (3) للموقف والتقرير اليومي",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "الاستمارة اليومية المعتمدة لرفع موقف التسجيل والتوزيع بالمركز.",
        "fillableId": None
    },
    {
        "file": "مطالعة نسب انجاز الجوالة.pdf",
        "title": "مطالعة نسب إنجاز الفرق الجوالة",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "كتاب مطالعة أسبوعي/شهري لحصر نسب إنجاز الفرق الميدانية وتحديث البطاقات.",
        "fillableId": None
    },
    {
        "file": "استمارة الاستقطاع المباشر.pdf",
        "title": "استمارة الاستقطاع المباشر للموظفين",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "pdf",
        "desc": "استمارة المحاسبة والشعبة المالية الخاصة بالاستقطاعات القانونية والرواتب.",
        "fillableId": None
    },
    {
        "file": "نموذج سجل حضور وانصراف 2928 2026.xlsx",
        "title": "سجل الحضور والانصراف اليومي للمركز 2026 (Excel)",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "xlsx",
        "desc": "شيت إكسل جاهز لتدوين أوقات حضور وانصراف وتواقيع موظفي مركز التسجيل.",
        "fillableId": None
    },
    {
        "file": "تسلسل الموظفين يكتب على النماذج.xlsx",
        "title": "جدول تسلسل وعناوين موظفي المركز (Excel)",
        "category": "reports",
        "categoryName": "التقارير اليومية والإدارية",
        "type": "xlsx",
        "desc": "قائمة تسلسل كودات وأسماء الموظفين المثبتة على المخاطبات الإدارية.",
        "fillableId": None
    },

    # 5. نماذج استرشادية توضيحية
    {
        "file": "2926  اجازة خاصة.pdf",
        "title": "نموذج استرشادي: طلب إجازة خاصة معبأ ومكتمل",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة نموذجية توضح للموظف ومسؤول المركز طريقة ملء وتوقيع الإجازة الخاصة.",
        "fillableId": None
    },
    {
        "file": "2926  اجازة الاعتيادبة  من 4 الى5.pdf",
        "title": "نموذج استرشادي: إجازة اعتيادية (4 إلى 5 أيام) معبأ",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة معبأة نموذجية توضح أسلوب تدوين التواريخ والموظف البديل.",
        "fillableId": None
    },
    {
        "file": "2926  اجازة اطويلة  من 6 الى اخرى.pdf",
        "title": "نموذج استرشادي: إجازة طويلة (6 أيام فأكثر) معبأ",
        "category": "samples",
        "categoryName": "نماذج استرشادية توضيحية",
        "type": "pdf",
        "desc": "نسخة معبأة نموذجية توضح مرفقات طلب الإجازة الطويلة المرفوعة للإدارة.",
        "fillableId": None
    }
]

forms_output = []
for idx, item in enumerate(forms_catalog, 1):
    src_file_path = os.path.join(src_dir, item["file"])
    dst_file_name = f"form_{idx}_{item['file']}"
    dst_file_path = os.path.join(dst_dir, dst_file_name)

    if os.path.exists(src_file_path):
        shutil.copy2(src_file_path, dst_file_path)
        sz = os.path.getsize(dst_file_path)
        sz_str = f"{round(sz/1024, 1)} KB" if sz < 1024*1024 else f"{round(sz/(1024*1024), 1)} MB"

        forms_output.append({
            "id": f"form_{idx:02d}",
            "title": item["title"],
            "category": item["category"],
            "categoryName": item["categoryName"],
            "fileType": item["type"],
            "fileName": dst_file_name,
            "originalName": item["file"],
            "fileSize": sz_str,
            "uploadDate": "2026-10-21",
            "updatedDate": "2026-10-21",
            "version": "2026",
            "description": item["desc"],
            "fillableTemplateId": item["fillableId"],
            "downloads": 0
        })
        print(f"✓ Copied: {item['title']} ({sz_str})")
    else:
        print(f"✗ Not found: {src_file_path}")

output_json = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\data\forms.json"
with open(output_json, "w", encoding="utf-8") as f:
    json.dump(forms_output, f, ensure_ascii=False, indent=2)

print(f"\nTotal official forms cataloged: {len(forms_output)}")
