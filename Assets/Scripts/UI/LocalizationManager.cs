using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameLanguage
{
    Persian = 0,
    English = 1
}

/// <summary>
/// Central manager for game localization (Persian / English).
/// Handles seamless collegiate dialogue translations and RTL/LTR switching.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public static event Action<GameLanguage> OnLanguageChanged;

    private static GameLanguage _currentLanguage = GameLanguage.Persian;

    public static GameLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set => SetLanguage(value);
    }

    public static bool IsRTL => _currentLanguage == GameLanguage.Persian;

    private static readonly Dictionary<string, string> _translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("LocalizationManager");
            Instance = go.AddComponent<LocalizationManager>();
            DontDestroyOnLoad(go);
            
            int savedLang = PlayerPrefs.GetInt("GameLanguage", (int)GameLanguage.Persian);
            _currentLanguage = (GameLanguage)savedLang;
            
            InitializeDictionary();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeDictionary();
    }

    private void Update()
    {
        // Shortcut: Press F3 to instantly toggle language in real-time during gameplay
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            ToggleLanguage();
            Debug.Log($"[LocalizationManager] Language switched to: {CurrentLanguage}");
        }
    }

    public static void SetLanguage(GameLanguage lang)
    {
        if (_currentLanguage == lang && PlayerPrefs.HasKey("GameLanguage")) return;

        _currentLanguage = lang;
        PlayerPrefs.SetInt("GameLanguage", (int)lang);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(_currentLanguage);
    }

    public static void ToggleLanguage()
    {
        SetLanguage(_currentLanguage == GameLanguage.Persian ? GameLanguage.English : GameLanguage.Persian);
    }

    /// <summary>
    /// Translates raw Persian dialogue or text key into collegiate English if English is selected.
    /// Returns the original text if Persian is selected or if no translation is found.
    /// </summary>
    public static string Translate(string rawText)
    {
        if (string.IsNullOrEmpty(rawText)) return "";
        if (_currentLanguage == GameLanguage.Persian) return rawText;

        string normalized = NormalizeKey(rawText);

        if (_translations.TryGetValue(normalized, out string translated))
        {
            return translated;
        }

        // Fuzzy fallback for multiline sentences or substring matches
        foreach (var pair in _translations)
        {
            if (pair.Key.Length > 10 && (normalized.Contains(pair.Key) || pair.Key.Contains(normalized)))
            {
                return pair.Value;
            }
        }

        return rawText;
    }

    public static string Get(string key, string fallback = "")
    {
        if (_currentLanguage == GameLanguage.Persian)
        {
            return !string.IsNullOrEmpty(fallback) ? fallback : key;
        }

        string normalized = NormalizeKey(key);
        if (_translations.TryGetValue(normalized, out string val))
        {
            return val;
        }

        return !string.IsNullOrEmpty(fallback) ? fallback : key;
    }

    public static string NormalizeKey(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        string cleaned = text.Replace("\r", "")
                             .Replace("\n", " ")
                             .Replace("ي", "ی")
                             .Replace("ك", "ک")
                             .Replace("۰", "0")
                             .Replace("۱", "1")
                             .Replace("۲", "2")
                             .Replace("۳", "3")
                             .Replace("۴", "4")
                             .Replace("۵", "5")
                             .Replace("۶", "6")
                             .Replace("۷", "7")
                             .Replace("۸", "8")
                             .Replace("۹", "9");

        // Collapse whitespace
        while (cleaned.Contains("  "))
        {
            cleaned = cleaned.Replace("  ", " ");
        }

        return cleaned.Trim();
    }

    private static void Add(string persianKey, string englishTranslation)
    {
        string norm = NormalizeKey(persianKey);
        if (!_translations.ContainsKey(norm))
        {
            _translations.Add(norm, englishTranslation);
        }
    }

    private static void InitializeDictionary()
    {
        if (_translations.Count > 0) return;

        // ==========================================
        // 1. Task Names & Core Objectives
        // ==========================================
        Add("برو به آموزش", "Go to Education Office");
        Add("برو پیش استاد مظاهری", "Go see Prof. Mazaheri");
        Add("برو پیش استاد ملک", "Go see Prof. Malek");
        Add("برو پیش استاد شکفته", "Go see Prof. Shekofteh");
        Add("برو پیش استاد علی‌اکبری", "Go see Prof. Ali Akbari");
        Add("برو پیش استاد علی اکبری", "Go see Prof. Ali Akbari");
        Add("برو پیش استاد صفایی", "Go see Prof. Safaei");
        Add("برو پیش استاد معصومی", "Go see Prof. Masoumi");
        Add("برو پیش استاد اسقایی", "Go see Prof. Asghari");
        Add("برو به کتابخانه", "Go to the Library");
        Add("همه مدارک جمع آوری شد!", "All documents successfully collected!");

        // ==========================================
        // 2. Common Prompts, Options & UI
        // ==========================================
        Add("بله", "Yes");
        Add("خیر", "No");
        Add("ممنون", "Thank you");
        Add("در چالش شکست خوردی", "Challenge Failed!");
        Add("خب مثل اینکه باید دوباره تلاش کنم", "Looks like I need to try again...");
        Add("خب بیا اینم مدرک", "Alright, here is your signed certificate.");
        Add("خب بیا اینم مدرکت", "Alright, here is your signed certificate.");
        Add("شما در همین طبقه هستید!", "You are already on this floor!");
        Add("آیا حاضر هستی که در چالش من شرکت کنی؟", "Are you ready to take on my academic challenge?");
        Add("آیا حاضر هستی که وارد کلاس بشی؟", "Are you ready to enter the classroom?");
        Add("تعداد تلاش ها :", "Attempts:");
        Add("مهندس بفرما معینی پور!", "Step right up, Engineer! Welcome to Moeinipour!");
        Add("بفرمایین معینی پور!", "Step right up, Moeinipour!");
        Add("خطر برق  گرفتگیص", "Danger: High Voltage Hazard!");
        Add("به به وقت غیبت عصرانه س!", "Ah, prime time for afternoon hallway gossip!");
        Add("واقعا خوابیدن تو نماز خونه بین کلاسا عالیه", "Seriously, taking a nap in the prayer room between lectures is heavenly.");

        // ==========================================
        // 3. Amozesh Phase 0 (Arrival & Orientation)
        // ==========================================
        Add("سلام اومدم برای تحویل مدرک", "Hello, I'm here to submit my registration documents.");
        Add("وقت ناهار نمازه برید بعدا بیاید", "It's lunch and prayer break right now. Please come back later.");
        Add("الان که 8 صبحه :| این مدارک من خدمت شما یه نگاه بندازید...", "It's literally 8 in the morning :| Here are my documents, please just take a look...");
        Add("الان که ۸ صبحه :| این مدارک من خدمت شما یه نگاه بندازید", "It's literally 8 in the morning :| Here are my documents, please just take a look...");
        Add("خاک بر سرت این همش مشکل داره تو چطوری بهشتی قبول شدی؟", "Good grief, these are riddled with errors! How on earth did you get accepted into Beheshti?!");
        Add("چه طرز حرف زدنه. حالا باید چیکار کنم؟", "Excuse me, what kind of attitude is that? What am I supposed to do now?");
        Add("ببین… بستگی به خودت داره میتونه 5 دقیقه طول بکشه یا 5 ساعت.", "Look... it entirely depends on you. It could take 5 minutes, or 5 hours.");
        Add("ببین… بستگی به خودت داره میتونه ۵ دقیقه طول بکشه یا ۵ ساعت.", "Look... it entirely depends on you. It could take 5 minutes, or 5 hours.");
        Add("یعنی چی؟", "What do you mean by that?");
        Add("هر کدوم از مدارک دست یکی از اساتیده،برو پیش هر کدوم از اساتید، مدارکت رو بگیر و بیارشون پیش من", "Each missing document is held by a professor. Go track them down, get their signatures, and bring them back to me.");
        Add("هر کدوم از مدارک دست یکی از اساتیده، برو پیش هر کدوم از اساتید، مدارکت رو بگیر و بیارشون پیش من", "Each missing document is held by a professor. Go track them down, get their signatures, and bring them back to me.");
        Add("وکی حالا باید چیکار کنم؟", "Alright, so where do I start?");
        Add("اوکی حالا باید چیکار کنم؟", "Alright, so where do I start?");
        Add("خب الان باید بری پیش استاد مظاهری، ایشون در کلاس 009 دارن کاربینی تدریس می کنن", "First, go see Professor Mazaheri. He's currently teaching Field Observation in Room 009.");
        Add("خب الان باید بری پیش استاد مظاهری، ایشون در کلاس ۰۰۹ دارن کاربینی تدریس می کنن", "First, go see Professor Mazaheri. He's currently teaching Field Observation in Room 009.");

        // ==========================================
        // 4. Amozesh Phase 1 (Mazaheri to Malek)
        // ==========================================
        Add("خب اینم از مدرک حالا میشه ثبت نامم کنی؟", "Alright, here's the document. Can you complete my registration now?");
        Add("هه فشار بخور، این اولیش بود هنوز کلییی کسری مدرک داری، از 8 مدرکی که باید تحویل بدی 6 تاش ناقصه.", "Don't hold your breath! That was just the first one—you're still missing a whole stack. Out of 8 required papers, 6 are still missing.");
        Add("هه فشار بخور، این اولیش بود هنوز کلییی کسری مدرک داری، از ۸ مدرکی که باید تحویل بدی ۶ تاش ناقصه.", "Don't hold your breath! That was just the first one—you're still missing a whole stack. Out of 8 required papers, 6 are still missing.");
        Add("فشار بخور،این اولیش بود هنوز کلی کسری مدرک داری", "Don't hold your breath! That was just the first one, you still have a whole pile missing.");
        Add("چیییی؟ یاعلیییی.", "Whaaat?! Oh man, you've got to be kidding me...");
        Add("همینس که هست.", "It is what it is, deal with it.");
        Add("عه چرا یه دفععه لهجه اصفهانی گرفتین؟ اصالتا اصفهانی هستین؟", "Wait, why did you suddenly switch to an Isfahani accent? Are you originally from Isfahan?");
        Add("عه چرا یه دفعه لهجه اصفهانی گرفتین؟ اصالتا اصفهانی هستین؟", "Wait, why did you suddenly switch to an Isfahani accent? Are you originally from Isfahan?");
        Add("ه تقصیر این دیالوگ نویسمه خودش اصفهانیه وگرنه خودم تورکم.", "Nah, blame our scriptwriter—he's Isfahani. Personally, I'm Azeri Turk.");
        Add("نه تقصیر این دیالوگ نویسمه خودش اصفهانیه وگرنه خودم تورکم.", "Nah, blame our scriptwriter—he's Isfahani. Personally, I'm Azeri Turk.");
        Add("واقعا نمیفهمم چی میگی", "I honestly have no clue what you're talking about...");
        Add("هیچی ولش کن", "Never mind, forget I said anything.");
        Add("خب میشه بگی استاد بعدی کیه؟", "Okay, can you just tell me who the next professor is?");
        Add("استاد بعدی دکتر ملک، دارن در کلاس 111 کاربینی تدریس می کنند.", "The next professor is Dr. Malek. He is currently lecturing in Room 111.");
        Add("استاد بعدی دکتر ملک، دارن در کلاس ۱۱۱ کاربینی تدریس می کنند.", "The next professor is Dr. Malek. He is currently lecturing in Room 111.");

        // ==========================================
        // 5. Amozesh Phase 2 (Malek to Shekofteh)
        // ==========================================
        Add("وای… چقدر… طول کشید اینم از مدرک سوم.", "Phew... that took forever! Here's the third document.");
        Add("خب اینم از مدرک دوم، به زور بیدارم واقعا..", "Well, here's the second document. I can barely keep my eyes open, honestly...");
        Add("الان دیگه واقعا وقت نماز و ناهاره. برو یه نیم ساعت دیگه بیا", "Now it's genuinely prayer and lunch time. Go take a walk and come back in half an hour.");
        Add("چی؟ خب اینطوری که من تا آخر شب هم نمی‌تونم ثبت نام رو تموم کنم.", "What?! If this keeps up, I won't finish registration by midnight!");
        Add("چی؟ خب اینطوری که من تا آخر شب هم نمی تونم ثبت نام رو تموم کنم.", "What?! If this keeps up, I won't finish registration by midnight!");
        Add("خب… به من چه؟", "Well... how is that my problem?");
        Add("خب یعنی اینکه آدم بیشو..", "Well, it means people should have some decency and not be so insensiti--");
        Add("چی؟ چی گفتی عزیزم؟", "Excuse me? What was that, sweetie?");
        Add("هیچی گفتم اگه میشه… امکانش هست… لطفا… کار های منو پیش ببرید.", "Nothing! I just said if it's possible... if you could kindly... please process my papers.");
        Add("حالا که خواهش می کنی یه کاری می کنم برات، بعدی دکتر علی اکبریه", "Since you asked so nicely, I'll do you a favor. Next up is Dr. Ali Akbari.");
        Add("حالا که خواهش می‌کنی یه کاری می‌کنم برات، بعدی دکتر علی اکبریه", "Since you asked so nicely, I'll do you a favor. Next up is Dr. Ali Akbari.");
        Add("اسم و فامیلشون علی اکبریه؟", "Is his full name Ali Akbari?");
        Add("نه، صادق علی اکبری کلاسشون هم در 101 داره برگزار میشه.", "No, Sadegh Ali Akbari. His class is currently being held in Room 101.");
        Add("نه، صادق علی اکبری کلاسشون هم در ۱۰۱ داره برگزار میشه.", "No, Sadegh Ali Akbari. His class is currently being held in Room 101.");

        // ==========================================
        // 6. Amozesh Phase 3 (Ali Akbari to Safaei)
        // ==========================================
        Add("خب اینم از استاد علی اکبری و مدرک چهارم.", "Alright, got through Dr. Ali Akbari and here is document number four.");
        Add("خب خب چطور بود؟", "Well well, how did that go?");
        Add("هنوز نیومده ازم کوئیز گرفتن:(", "I barely set foot in the room and he gave me a pop quiz! :(");
        Add("درستش هم همینه، حالا کجاشو دیدی، ددلاین ها در انتظارته.", "That's standard procedure! You haven't seen anything yet—just wait until assignment deadlines hit.");
        Add("هعی.. خب حالا استاد بعدی کیه؟", "Sigh... Alright, who's the next professor?");
        Add("استاد بعدی… دکتر صفایی که در کلاس 009 هستند.", "The next professor is Dr. Safaei. He is currently in Room 009.");
        Add("استاد بعدی… دکتر صفایی که در کلاس ۰۰۹ هستند.", "The next professor is Dr. Safaei. He is currently in Room 009.");

        // ==========================================
        // 7. Amozesh Phase 4 (Safaei to Masoumi)
        // ==========================================
        Add("اینم از paper پنجم که دکتر صفایی امضا کردن", "Here's paper number five, signed by Dr. Safaei.");
        Add("خسته نباشی، فقط 2 تا مدرک دیگه مونده!", "Good job! Only 2 more documents left to go!");
        Add("خسته نباشی، فقط ۲ تا مدرک دیگه مونده!", "Good job! Only 2 more documents left to go!");
        Add("مرسی که حداقل روحیه میدی، استاد بعدی کیه؟", "Thanks for the encouragement at least. Who is the next professor?");
        Add("استاد معصومی دارن در کلاس 108 تدریس می کنن زود برو تا کلاسش تموم نشده.", "Professor Masoumi is teaching in Room 108. Hurry up before his lecture ends.");
        Add("استاد معصومی دارن در کلاس ۱۰۸ تدریس می کنن زود برو تا کلاسش تموم نشده.", "Professor Masoumi is teaching in Room 108. Hurry up before his lecture ends.");

        // ==========================================
        // 8. Amozesh Phase 5 (Masoumi to Asghari)
        // ==========================================
        Add("اینم از هفتمین مدرک. خداروشکررر که تموم شد.", "Here's the seventh document. Thank goodness, we're finally done!");
        Add("می بینم که وضع ریاضیت هم مثل آیندت داغونه.", "I see your math skills are just as bleak as your academic future.");
        Add("چرا؟", "Why?!");
        Add("نابغه این 6امین مدرک بود، هنوز یه مدرک دیگه مونده", "Genius, that was only the 6th document. You still have one more left.");
        Add("نابغه این ۶امین مدرک بود، هنوز یه مدرک دیگه مونده", "Genius, that was only the 6th document. You still have one more left.");
        Add("ای بابااااا", "Oh come onnnnn!");
        Add("هی هی هی ها ها ها", "Hehehe hahaha!");
        Add("فقط بگو استاد بعدی کیه؟", "Just tell me who the next professor is, please.");
        Add("بعدی استاد اسقایی، در کلاس 112 مشغول تدریس هستن.", "Next is Professor Asghari. He is lecturing in Room 112.");
        Add("عدی استاد اسقایی، در کلاس۱۱۲ مشغول تدریس هستن.", "Next is Professor Asghari. He is lecturing in Room 112.");
        Add("حله پس من رفتم.", "Got it, on my way.");
        Add("فقط یه چیزی، ایشون یکمی حساسن که سر کلاس مزاحمشون بشی، باید دزدکی بری مدرکتو برداری.", "Just one thing: he is extremely strict about classroom interruptions. You'll have to sneak in quietly to retrieve your paper.");
        Add("مگه قرار نبود فقط قبولی تو دانشگاه سخت باشه؟ پس این ثبت نامش چی میگه:(((", "Wasn't getting into college supposed to be the hard part?! Why is this registration such an ordeal?! :(((");
        Add("حالا بذار بعدشو ببینی:)))", "Haha, just wait until the actual semester starts! :)))");

        // ==========================================
        // 9. Amozesh Phase 6 (Asghari to Twist 404)
        // ==========================================
        Add("خب دیگه…تموم شد دیگه آره؟", "Alright then... we're finally done, right?");
        Add("بذار یه نگاه بندازم…. اوه. بدبخت شدی…", "Let me take a look... Uh oh. You're in deep trouble...");
        Add("یعنی چی؟ چی شده؟", "What do you mean?! What happened?!");
        Add("دستم خورد همه مدارکتو اشتباهی پاک کردم حالا باید همشو از اول بگیری", "My hand slipped and I accidentally deleted all your records. You'll have to get them all signed from scratch.");
        Add("چیییی؟:/", "WHAAAT?!?! :/");
        Add("نمی دونم والا هرچی سرچ می کنم می زنه 404NotFound.", "I don't know, whatever I search in the portal comes back with '404 Not Found'!");
        Add("خب یه کاری کننننننن", "Well DO SOMETHING about it, pleaseeee!");
        Add("همینه که هست. وقت ناهار و نماز اومدی دو قورت و نیمت هم باقیه؟", "It is what it is. You interrupt my lunch break and now you're giving me an attitude?");
        Add("یعنی راه دیگه ای نیست؟ دوباره برم پیش همه اساتید؟", "Is there really no other way?! I have to go through every single professor again?!");
        Add("چرا یه راه دیگه هست… ببین داخل کتابخونه بایگانی مدارک هست، برو طبقه سوم کپی مدارکتو بگیر، بیار.", "Well, there is an alternative... Head over to the library archives on the 3rd floor; grab backup copies of your records and bring them back.");

        // ==========================================
        // 10. Amozesh Phase 7 (Finale)
        // ==========================================
        Add("خب اینم از کپی مدارکم، دیگه چی؟ باز باید چیکار کنم؟ حالا هم حتما سامانه قطعه آره؟", "Here are the archive copies of all my papers. What now? Let me guess, the system is offline again?");
        Add("سامانه که یه 3 سالی هست قطعه گل مدارکتو بده ببینم", "Sweetheart, the university portal has been offline for 3 years! Hand over your papers, let me see.");
        Add("سامانه که یه ۳ سالی هست قطعه گل مدارکتو بده ببینم", "Sweetheart, the university portal has been offline for 3 years! Hand over your papers, let me see.");
        Add("بفرمایید", "Here you go.");
        Add("خب… اینم از این عزیزم همه چیز درست به نظر میرسه فقط یه چیزی…", "Alright... everything looks in order here, dear. Just one little thing...");

        // ==========================================
        // 11. Library & Archive (TA Grading Dialogues)
        // ==========================================
        Add("سلام من از طرف آموزش اومدم گفتن بیام از بایگانی مدارکمو بگیرم", "Hello, Education sent me. They told me I could retrieve backup copies of my registration documents from the archives.");
        Add("ای بابا یکی دیگه؟ تو نفر 65امی هستی که امروز اومدی. چرا اینقدر شما حواس پرتین؟", "Oh great, another one? You're the 65th student to show up today! Why are freshmen so absent-minded?");
        Add("ای بابا یکی دیگه؟ تو نفر ۶۵امی هستی که امروز اومدی. چرا اینقدر شما حواس پرتین؟", "Oh great, another one? You're the 65th student to show up today! Why are freshmen so absent-minded?");
        Add("آره، آره تو راست میگی. آموزش مدرکتو گم کرده مگه نه؟", "Yeah, yeah, sure you did. Education 'lost' your documents again, right?");
        Add("ولی آموش مدرک منو..", "But Education really did--");
        Add("ولی آموزش مدرک منو..", "But Education really did--");
        Add("ببین من سرم خیلی شلوغه، تو بیا جای من برگه بچه ها‌ رو تصحیح کن تا مدارکتو پیدا کنم", "Look, I'm completely swamped. Take over grading these student exam papers for me while I dig through the files for your documents.");
        Add("باوشه ولی چطوری تصحیح کنم؟ من که اصلا چیزی بلد نیستم", "Okay, but how do I grade them? I don't know the first thing about grading CS exams!");
        Add("هرطور دلت خواست اصلا خواستی تاس بریز نمره بده", "Grade however you like—roll a die if you have to, just get through them!");
        Add("عجب! مش رجب! یعنی اینطوری داخل دانشگاه نمره می دن؟", "Good grief! Is this seriously how university exams are graded?!");
        Add("سوال نپرس تصحیح کن! فقط بعد از تصحیح برگه ها رو درست دسته بندی کن", "Quit asking philosophical questions and start grading! Just make sure you categorize the papers correctly into High, Medium, and Low.");
        Add("بابا به خدا..", "Oh, come on...");
        Add("خب آماده ای برگه ها رو تصحیح کنی؟", "So, are you ready to grade these exam papers?");
        Add("از این تصمیم مطمئنی؟", "Are you sure about this decision?");
        Add("خب اینم از مدرک هات راستی یه 2 تا نامه دیدم دستت اونا چی بودن؟", "Alright, here are your signed certificates. By the way, I saw two letters in your hand—what were those?");
        Add("خب اینم از مدرک هات راستی یه ۲ تا نامه دیدم دستت اونا چی بودن؟", "Alright, here are your signed certificates. By the way, I saw two letters in your hand—what were those?");
        Add("چیز خاصی نبودن… خودتون بعدا میفهمین.... فعلا!!!", "Nothing important... you'll find out in due time. Catch you later!");
        Add("سلام متاسفانه شما در تصحیح درست عمل نکردید و بیش از حد بچه ها رو پاس کردید، متاسفانه مجبوریم عذر شما رو بخوایم!", "Official Notice: Regrettably, your evaluation was far too lenient—you passed way too many students! The department has no choice but to terminate your position.");
        Add("سلام متاسفانه شما در تصحیح درست عمل نکردید و خیلی از بچه ها رو انداختید، شکایات هایی از شما شده و متاسفانه مجبوریم عذر شما رو بخوایم!", "Official Notice: Unfortunately, your grading standards were excessively harsh; an alarming number of students failed. Formal grievances have been filed, so we must relieve you of your duties.");
        Add("سلام متاسفانه شما در تصحیح خوب عمل کردید ولی خب از اونجایی که خوب عمل کردید مجبوریم اخراجتون کنیم.", "Official Notice: You actually performed exceptionally well in your grading duties... but ironically, because your standards were too accurate, the committee has decided to dismiss you anyway.");
        Add("اسم: سیمین فرنوش شماره دانشجویی: 97404238 نمره تصحیح 19.75 سلام استاد خسته نباشین خیلی ممنون از تدریس خوبی که داشتید. شما یکی از بهترین اساتیدی بودید که در این ترم و حتی در دوره کارشناسی باهاشون کلاس داشتم، امیدوارم هرجا که هستید سالم و سلامت باشید", "Name: Simin Farnoosh | Student ID: 97404238 | Grade: 19.75\n'Dear Professor, thank you so much for your inspiring lectures this semester. You have been one of the most remarkable instructors throughout my undergraduate studies. Wishing you health and scholarly success!'");
        Add("اسم: سیمین فرنوش شماره دانشجویی: ۹۷۴۰۴۲۳۸ نمره تصحیح ۱۹.۷۵ سلام استاد خسته نباشین خیلی ممنون از تدریس خوبی که داشتید. شما یکی از بهترین اساتیدی بودید که در این ترم و حتی در دوره کارشناسی باهاشون کلاس داشتم، امیدوارم هرجا که هستید سالم و سلامت باشید", "Name: Simin Farnoosh | Student ID: 97404238 | Grade: 19.75\n'Dear Professor, thank you so much for your inspiring lectures this semester. You have been one of the most remarkable instructors throughout my undergraduate studies. Wishing you health and scholarly success!'");

        // ==========================================
        // 12. Professor Safaei (Logic Lecture & Options)
        // ==========================================
        Add("خب خوش اومدید بچه ها ما اومدیم داخل این کلاس درمورد discrete mathmatics صحبت کنیم! درسته که جلسه اوله ولی از همین الان عقبیم، پس بدون هیچ waste of timeیی بریم سر درس: subject این جلسه علم logic هست: علم منطق مجموعه ای از قواعد منطقی هست که ما اجازه میدن درمورد منطق، منطقی صحبت کنیم.",
            "Welcome everyone! We're here to dive into Discrete Mathematics. I know it's only session one, but we are already running behind schedule! So without any waste of time, let's jump straight to our subject: Logic! The science of logic is a formal framework that allows us to reason about reasoning itself.");

        Add("ببینید علم منطق علم ساده ای نیست، مثلا یه مثال برای شما بزنم: شما تا حالا به این جمله فکر کردید آدم حیوان است، دانشجو آدم است: پس دانشجو حیوان است؟ خب این جمله خیلی نکات پنهانی داخلش هست البته من خودم خیلی در این زمینه ادعایی ندارم، سر رشته واقعی در این زمینه رو دکتر معصومی دارند ولی اگر خواستید بیاید با هم دیگه در این زمینه پیپر می دیم.",
            "Now, formal logic is deceptively subtle. Let me give you a classic example: Have you ever considered the syllogism: 'Humans are biological organisms; students are humans; therefore students are biological organisms'? There are deep epistemological subtleties here. I don't claim to be the foremost authority—Dr. Masoumi is the real expert in this domain—but if anyone is interested, we can co-author a paper on it.");

        Add("paper دادن از اوجب واجبات است اگر دوست داشتید می تونیم با هم دیگه یه tea talk داشته باشیم و درمورد subject مورد interest تون صحبت کنیم همین چند وقت پیش بود یه نفر اومد پیشم tea talk کنیم وسط صحبتمون بود که متوجه شدم دچار افسردگی degree 3 شده و می خواد sucide کنه، صحبت کردیم و قانعش کردم به جای خودکشی بره تیمارستان. خب that's a win isn't it?",
            "Publishing papers is the absolute highest calling of academia! If you're interested, we can schedule an afternoon tea talk and brainstorm your research interests. Just recently, a student came to my office for a tea talk; midway through, I realized they were suffering from stage-3 severe depression and contemplating drastic measures. We had an extensive dialogue, and I persuaded them to seek clinical counseling instead. Well, that's a decisive win, isn't it?");

        Add("bale dashtam migoftam kholase be careful bashid ke ye vaght ab beheshti( i mean behestri water for thisnoobs who don't undertand fingilish) ro nakohrid ke corroput mishid. In harf man ro hesabi be gosh besparid. Movazeb bashid!",
            "Indeed, as I was saying: be extremely cautious never to drink the raw tap water up here on the Beheshti mountain, or you'll get corrupted! Take my word as essential academic advice. Stay vigilant!");

        // Safaei Option responses
        Add("استاد میشه یکمی درمورد تجربیاتتون در منطق صحبت کنین؟", "Professor, could you share more about your personal research in mathematical logic?");
        Add("استاد خسته نباشین، برای همین جلسه بسه", "Thank you professor, that will be enough for today's session.");
        Add("استاد میشه یه دور دیگه این موضوع رو توضیح بدید؟", "Professor, could you elucidate that theoretical point once more?");
        Add("استاد خسته نباشین", "Thank you for the lecture, Professor.");
        Add("استاد میشه درمورد پیپر دادن یه ذره صحبت کنین؟", "Professor, could we elaborate on publishing academic papers?");
        Add("استاد طبق این حرفتون اگر گاوداری مکان باشد و دانشگاه گاوداری باشد در این صورت دانشگاه مکان است؟", "Professor, by that syllogism, if a cattle ranch is a location, and this university operates like a cattle ranch, does that formally prove the university is a physical location?");
        Add("استاد این چقدر مربوط به درسه واقعا!", "Professor, how deeply relevant to the syllabus is this really?!");
        Add("mn dgh nmt.", "I can't take this anymore...");
        Add("یره چی‌ کو؟", "What on earth are you talking about?!");
        Add("کاملا موافقم", "I wholeheartedly concur.");
        Add("استاد بس کنم خستم.", "Professor, please, I'm completely exhausted.");
        Add("باع", "Baah!");

        // ==========================================
        // 13. Professor Masoumi (Syllogisms & Logic Puzzles)
        // ==========================================
        Add("خب بیا بیا اینجا ببینم پسر خوب این مدرک مدرک که میگی چی هست؟ اصلا کی گفته مدرکی وجود داره؟", "Well now, come over here, young scholar. This 'certificate' you keep asking about—what constitutes its objective reality? Who even proved a certificate exists in the first place?");
        Add("من داخل سامانه ثبت نام کردم", "I submitted my formal registration into the portal.");
        Add("سامانه مدرک من رو ثبت کرده", "The portal logged and authenticated my certificate.");
        Add("پس مدرکی وجود داره", "Therefore, an authentic certificate objectively exists.");
        Add("خب باشه اصلا راست میگی، یه مدرکی وجود داره ولی کی گفته اینجا دانشگاهه؟ شاید اشتباه اومدی اصلا اینجا دانشگاه نیست", "Fair enough, suppose a document exists. But what axiomatic proof do you have that this building is an actual university? Perhaps you wandered into an illusion and this is no academic institution at all.");
        Add("دانشگاه جاییه درس کارشناسی، ارشد، دکتری تدریس میشه", "A university is defined as an institution where undergraduate and postgraduate curricula are taught.");
        Add("اگه اینجا درس تدریس میشه", "If accredited academic curricula are indeed being taught here...");
        Add("پس دانشگاهه", "Then by definition, this institution is a university.");
        Add("وایسا ببینم اصلا یه سوال گنده تر کی گفته من استاد این دانشگاهم؟ شاید من یه پرنده باشم. در بازه، باز هم پرندس. پس پرندس. پس… من گرگم.", "Hold on! An even more profound epistemological conundrum: Who proved I am a faculty professor here? Perhaps I am a majestic falcon. The door is open; a falcon is airborne. Thus, airborne implies falcon... therefore, I am a wolf!");
        Add("تو اینجا تدریس می کنی", "You actively deliver lectures and evaluate students here.");
        Add("اگر کسی اینجا تدریس می کنه پس استاده", "If an individual delivers academic lectures here, they possess professorial status.");
        Add("در نتیجه تو استاد هستی", "Consequently, you are proven beyond doubt to be our professor!");

        // ==========================================
        // 14. Other Professors (Mazaheri, Malek, Ali Akbari, Asghari)
        // ==========================================
        Add("اینجا باید باگ های لینوکس رو حذف کنیم تا سیستم عامل درست کار کنه!", "We need to debug these kernel bugs in Linux so the operating system compiles correctly!");
        Add("کلاسای استاد ملک خیلی خواب آوره ولی حواست باشه خوابت نبره! وگرنه مدرکت پر!", "Dr. Malek's lecture is overwhelmingly sleep-inducing, but stay alert! If you doze off, kiss your certificate goodbye!");
        Add("کلاسای استاد علی اکبری باید هی تمرین رو ارسال کنی تا نمره کامل بگیری", "In Dr. Ali Akbari's class, you must continuously resubmit your assignments until you achieve a perfect score.");
        Add("سر کلاس استاد صفایی نباید سوال نامربوط بپرسی وگرنه مدرکت رو نمیده", "Never ask off-topic questions in Dr. Safaei's class, or he will refuse to sign your papers.");
        Add("کلاس استاد معصومی باید همه چیز رو اثبات کنی وگرنه مدرکتو بهت نمیده!", "In Dr. Masoumi's class, every single assertion must be mathematically proven, or no certificate for you!");
        Add("وقتی که استاد اسقایی حواسش نیست سریع برو مدرکتو بگیر و فرار کن!", "Wait until Professor Asghari turns his back—then swiftly grab your certificate and make a dash for the exit!");
        Add("اگه دنبال مدرکت اومدی با مسئول کتابخونه صحبت کن", "If you're seeking your paperwork, go consult the archive librarian.");

        // ==========================================
        // 15. All Campus, Floor & Hallway Student NPCs (56 Lines)
        // ==========================================
        Add("باورم نمیشه رزرو غذای هفته دیگه قراره بهمون کباب کوبیده بدن! چه غذاهای شیکی", "I can't believe next week's cafeteria menu actually has Koobideh kebab! Feeling fancy!");
        Add("با من حرف نزن! من در حال تمرکز کردنم", "Don't talk to me! I am in the middle of intense theoretical concentration.");
        Add("ما از دیشب چادر زده بودیم دم دانشکده که بتونیم ثبت نام کنیم!", "We literally pitched a tent outside the faculty building last night just to register!");
        Add("چند ساعت دیگه باید منتظر ثبت نام بمونیم؟ من 3 ساعته تو صفم", "How many more hours do we have to wait for registration? I've been in this line for 3 hours!");
        Add("چند ساعت دیگه باید منتظر ثبت نام بمونیم؟ من ۳ ساعته تو صفم", "How many more hours do we have to wait for registration? I've been in this line for 3 hours!");
        Add("خیلی خوشحالم بهشتی قبول شدم! دیگه فقط عشق و حال می کنیم.", "I'm so thrilled I got into Beheshti! Now it's just smooth sailing and fun from here.");
        Add("به نظرت تو زنگ تفریح ها می تونیم فوتبال بازی کنیم؟", "Do you think we can play soccer during recess between lectures?");
        Add("راسته می گن دانشگاه اینترنتش نامحدوده؟", "Is it true that campus Wi-Fi has unlimited high-speed bandwidth?");
        Add("این ترم که نمیشه ولی ترم دیگه 24 واحد برمی دارم 7 ترمه دانشگاه رو تموم می کنم.", "Not this semester, but next term I'm enrolling in 24 credits and graduating in 7 semesters flat.");
        Add("این ترم که نمیشه ولی ترم دیگه ۲۴ واحد برمی دارم ۷ ترمه دانشگاه رو تموم می کنم.", "Not this semester, but next term I'm enrolling in 24 credits and graduating in 7 semesters flat.");
        Add("میگن بعضی کلاس هامون داخل پردیس عباس پور برگزار میشه راست میگن؟", "They say some of our lectures are held way over at the Abbaspour campus. Is that true?");
        Add("هعییی اگه فقط یه تست بیشتر زده بودم… الان شریف بودم", "Sigh... if I had just answered one more question correctly on the entrance exam, I'd be at Sharif right now...");
        Add("انجمن علمی خیلییی جای باحالیه", "The Student Scientific Association is such an awesome place!");
        Add("اگه می خوای توی انجمن عضو شی، باید توی انتخاباتش رای بیاری", "If you want to join the association board, you need to campaign and win the student elections.");
        Add("به نظرت امسال sliksong برنده میشه یا clair obscur ؟", "Do you think Silksong or Clair Obscur is taking Game of the Year?");
        Add("برای اینکه بیای داخل گیم لب باید ترم 2 پروژه گیم برداری", "To get access to the Game Lab, you have to take the Game Project in semester 2.");
        Add("برای اینکه بیای داخل گیم لب باید ترم ۲ پروژه گیم برداری", "To get access to the Game Lab, you have to take the Game Project in semester 2.");
        Add("چطوری تونستی بیای اینجا؟ مگه درش اثر انگشت نمی خواد؟", "How did you even get in here? Doesn't this lab door require biometric fingerprint access?");
        Add("انجمن ما برقیا رو دیدی ؟", "Have you checked out our Electrical Engineering association lounge?");
        Add("میدونستی مهندسی پزشکی هم انجمن داره؟", "Did you know Biomedical Engineering has its own student association too?");
        Add("ببخشید من ارائه دارم نمی تونم صحبت کنم", "Sorry, I have a presentation starting in five minutes, I can't talk!");
        Add("اهلا و سهلا! انا دانشجوی الاتباع", "Ahlan wa Sahlan! Greetings, I am an international student.");
        Add("زوجی یا فرد؟ من با گروه زوج حرفی ندارم", "Are you even or odd cohort? I don't speak to even-group people.");
        Add("هرکاری می کنی ترم2 پروژه گیم برندار", "Whatever you do, do NOT pick the game project in term 2!");
        Add("هرکاری می کنی ترم۲ پروژه گیم برندار", "Whatever you do, do NOT pick the game project in term 2!");
        Add("بححح ترمکی هستی؟ قدر ترم 1 رو بدون", "Well, well, a fresh freshman? Cherish your first semester, rookie!");
        Add("بححح ترمکی هستی؟ قدر ترم ۱ رو بدون", "Well, well, a fresh freshman? Cherish your first semester, rookie!");
        Add("زوجی؟ من از فردی جماعت خوشم نمیاد", "You're from the even cohort? I can't stand the odd-group folks.");
        Add("کتابخونه طبقه بالاعه", "The library is on the upper floor.");
        Add("کتابخونه طبقه پایینه ها", "The library is on the lower floor, you know.");
        Add("48 ساعته که نخوابیدم امیدوارم پروژه رو تمدید بزنه", "I haven't slept in 48 hours. I pray the professor extends the project deadline!");
        Add("۴۸ ساعته که نخوابیدم امیدوارم پروژه رو تمدید بزنه", "I haven't slept in 48 hours. I pray the professor extends the project deadline!");
        Add("ترمکی هستی؟ می دونم سخته ولی سعی کن بالانس رو نگه داری", "Freshman, huh? I know it's overwhelming, but try to maintain a healthy study-life balance.");
        Add("بهشتی سریع سرد میشه لباس بپوش جوون", "The mountain breeze at Beheshti gets freezing fast—bundle up, rookie!");
        Add("مشاور کنکور فریبم داد، لیسانس عذابم می دهد، از فوق لیسانس وحشت دارم.", "My college counselor deceived me, my Bachelor's is pure torment, and I'm terrified of grad school.");
        Add("این طبقه فقط اتاق اساتیده با کی کار داری؟", "This entire floor is faculty offices. Which professor are you looking for?");
        Add("گم شدی کوچولو؟", "Lost your way, kiddo?");
        Add("ببخشید دانشکده شیمی همینجاس؟", "Excuse me, is this the Chemistry faculty?");
        Add("آروم حرف بزن", "Keep your voice down!");
        Add("هیس!!!", "Shhh!!!");
        Add("هیس! آروم حرف بزن", "Shhh! Keep your voice down!");
        Add("زود بجنب یه میز که پریز داره پیدا کن تا همشون پر نشده", "Hurry up! Grab a study table with a power outlet before they're all taken!");
        Add("ببخشید من کار دارم", "Sorry, I'm quite busy right now.");
        Add("من 21 واحد گرفتم می خوام یه 3 واحد دیگه بزنم رند شه.", "I enrolled in 21 credits; I might just add 3 more so it rounds out to 24.");
        Add("من ۲۱ واحد گرفتم می خوام یه ۳ واحد دیگه بزنم رند شه.", "I enrolled in 21 credits; I might just add 3 more so it rounds out to 24.");
        Add("حتما بخش اخبار رو بخون!", "Make sure to read the campus news column!");
        Add("تاحالا نشریه پیکسل رو خوندی ؟", "Have you ever read the Pixel student magazine?");
        Add("واقعا هوای بهشتی تکه!", "The air up here on the Beheshti mountain is truly unparalleled!");
        Add("پشت دانشگاه هم جای دنجیه مگه نه؟", "The backyard spot behind the faculty is so cozy, right?");
        Add("به به ورودی جدید از کدوم شهر اومدی ؟", "Welcome, new student! Which city did you move here from?");
        Add("جدا این دانشگاه رو برا ادما ساختن؟ بز کوهی هم اینجا کم میاره", "Was this campus built for humans? Even a mountain goat would run out of breath climbing these hills!");
        Add("من پشت سایت کمین کرده بودم وقتی سایت باز شد نوشت نشست شما به پایان رسیده:(", "I was camping on the portal, but the second it opened, it gave me: 'Your session has expired' :(");
        Add("دیشب خواب دیدم 17 واحد دارم، همونجا فهمیدم دارم خواب می بینم", "Last night I dreamt I successfully registered for 17 credits; that's how I instantly knew I was dreaming.");
        Add("دیشب خواب دیدم ۱۷ واحد دارم، همونجا فهمیدم دارم خواب می بینم", "Last night I dreamt I successfully registered for 17 credits; that's how I instantly knew I was dreaming.");
        Add("بابا من کلا 3 واحد دارم الان باید چیکار کنم؟", "Man, I only managed to register for 3 credits! What am I supposed to do now?!");
        Add("بابا من کلا ۳ واحد دارم الان باید چیکار کنم؟", "Man, I only managed to register for 3 credits! What am I supposed to do now?!");
        Add("ببین ترمکی جان، چیزی به نام انتخاب واحد نداریم همش انتصاب واحده", "Listen freshman, there's no such thing as 'course selection'—it's strictly 'course imposition'!");
        Add("دلم برای مدرسه که برنامه هفتگی می دادن تنگ شده", "I actually miss high school when they just handed us a pre-made weekly schedule.");
        Add("آقا این جبر خطی ما چی شد؟87!", "Hey, what happened to our Linear Algebra class?! Course code 87!");
        Add("آقا این جبر خطی ما چی شد؟۸۷!", "Hey, what happened to our Linear Algebra class?! Course code 87!");
        Add("حاجی ساعت 12 ددلاین دارم هنوز یه سوال هم حل نکردم", "Bro, my assignment is due at midnight and I haven't solved a single question yet!");
        Add("حاجی ساعت ۱۲ ددلاین دارم هنوز یه سوال هم حل نکردم", "Bro, my assignment is due at midnight and I haven't solved a single question yet!");
        Add("اینجا فقط تا ساعت 15 بازه", "The computer lab is only open until 3:00 PM.");
        Add("اینجا فقط تا ساعت ۱۵ بازه", "The computer lab is only open until 3:00 PM.");
        Add("ای بابا اینترنت دانشگاه هم که فیلتره", "Come on, even the university Wi-Fi is heavily filtered!");
        Add("واییییی از دست نت این دانشگاه خیلی کنده.", "Ugh, this campus network is excruciatingly slow!");
    }
}
