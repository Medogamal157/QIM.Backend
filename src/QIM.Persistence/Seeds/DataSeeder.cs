using Microsoft.EntityFrameworkCore;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities;
using QIM.Persistence.Contexts;

namespace QIM.Persistence.Seeds;

public static class DataSeeder
{
    public static async Task SeedAsync(QimDbContext context)
    {
        await SeedCountriesAsync(context);
        await SeedCitiesAsync(context);
        await SeedDistrictsAsync(context);
        await SeedActivitiesAsync(context);
        await SeedSpecialitiesAsync(context);
        await SeedPlatformSettingsAsync(context);
        await SeedEgyptCitiesAsync(context);
        await SeedEgyptDistrictsAsync(context);
        await SeedBusinessesAsync(context);
        await SeedKeywordsAsync(context);
        await SeedReviewsAsync(context);
        await SeedBlogPostsAsync(context);
        await SeedClaimsAsync(context);
        await SeedContactsAsync(context);
        await SeedSuggestionsAsync(context);
        await SeedAdvertisementsAsync(context);
    }

    private static async Task SeedCountriesAsync(QimDbContext context)
    {
        if (await context.Countries.AnyAsync()) return;

        var countries = new[]
        {
            new Country { NameAr = "الأردن", NameEn = "Jordan", IsDefault = true, SortOrder = 1, IsEnabled = true },
            new Country { NameAr = "مصر", NameEn = "Egypt", SortOrder = 2, IsEnabled = true },
            new Country { NameAr = "السعودية", NameEn = "Saudi Arabia", SortOrder = 3, IsEnabled = true },
            new Country { NameAr = "الإمارات", NameEn = "UAE", SortOrder = 4, IsEnabled = true },
            new Country { NameAr = "الكويت", NameEn = "Kuwait", SortOrder = 5, IsEnabled = true },
            new Country { NameAr = "قطر", NameEn = "Qatar", SortOrder = 6, IsEnabled = true },
        };

        context.Countries.AddRange(countries);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCitiesAsync(QimDbContext context)
    {
        if (await context.Cities.AnyAsync()) return;

        var jordan = await context.Countries.FirstOrDefaultAsync(c => c.NameEn == "Jordan");
        if (jordan == null) return;

        var cities = new[]
        {
            new City { NameAr = "عمّان", NameEn = "Amman", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "إربد", NameEn = "Irbid", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "الزرقاء", NameEn = "Zarqa", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "العقبة", NameEn = "Aqaba", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "السلط", NameEn = "Salt", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "المفرق", NameEn = "Mafraq", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "جرش", NameEn = "Jerash", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "عجلون", NameEn = "Ajloun", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "الكرك", NameEn = "Karak", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "الطفيلة", NameEn = "Tafilah", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "مأدبا", NameEn = "Madaba", CountryId = jordan.Id, IsEnabled = true },
            new City { NameAr = "معان", NameEn = "Ma'an", CountryId = jordan.Id, IsEnabled = true },
        };

        context.Cities.AddRange(cities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDistrictsAsync(QimDbContext context)
    {
        if (await context.Districts.AnyAsync()) return;

        var amman = await context.Cities.FirstOrDefaultAsync(c => c.NameEn == "Amman");
        if (amman == null) return;

        var districts = new[]
        {
            new District { NameAr = "العبدلي", NameEn = "Abdali", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "الصويفية", NameEn = "Sweifieh", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "جبل عمان", NameEn = "Jabal Amman", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "الشميساني", NameEn = "Shmeisani", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "الرابية", NameEn = "Rabieh", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "دير غبار", NameEn = "Deir Ghbar", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "تلاع العلي", NameEn = "Tla'a Al-Ali", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "خلدا", NameEn = "Khalda", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "الجبيهة", NameEn = "Jubeiha", CityId = amman.Id, IsEnabled = true },
            new District { NameAr = "أبو نصير", NameEn = "Abu Nsair", CityId = amman.Id, IsEnabled = true },
        };

        context.Districts.AddRange(districts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedActivitiesAsync(QimDbContext context)
    {
        if (await context.Activities.AnyAsync()) return;

        var activities = new[]
        {
            new Activity { NameAr = "المقاولات والبناء", NameEn = "Construction", IconUrl = null, Color = "#F59E0B", SortOrder = 1, IsEnabled = true },
            new Activity { NameAr = "العقارات", NameEn = "Real Estate", IconUrl = null, Color = "#3B82F6", SortOrder = 2, IsEnabled = true },
            new Activity { NameAr = "الصيانة", NameEn = "Maintenance", IconUrl = null, Color = "#EF4444", SortOrder = 3, IsEnabled = true },
            new Activity { NameAr = "التكنولوجيا", NameEn = "Technology", IconUrl = null, Color = "#8B5CF6", SortOrder = 4, IsEnabled = true },
            new Activity { NameAr = "التعليم", NameEn = "Education", IconUrl = null, Color = "#10B981", SortOrder = 5, IsEnabled = true },
            new Activity { NameAr = "الصحة", NameEn = "Healthcare", IconUrl = null, Color = "#EC4899", SortOrder = 6, IsEnabled = true },
            new Activity { NameAr = "المطاعم والمقاهي", NameEn = "Restaurants & Cafes", IconUrl = null, Color = "#F97316", SortOrder = 7, IsEnabled = true },
            new Activity { NameAr = "السياحة والسفر", NameEn = "Tourism & Travel", IconUrl = null, Color = "#06B6D4", SortOrder = 8, IsEnabled = true },
            new Activity { NameAr = "الخدمات المالية", NameEn = "Financial Services", IconUrl = null, Color = "#14B8A6", SortOrder = 9, IsEnabled = true },
            new Activity { NameAr = "السيارات", NameEn = "Automotive", IconUrl = null, Color = "#6366F1", SortOrder = 10, IsEnabled = true },
        };

        context.Activities.AddRange(activities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSpecialitiesAsync(QimDbContext context)
    {
        if (await context.Specialities.AnyAsync()) return;

        var activities = await context.Activities.ToListAsync();
        var specialities = new List<Speciality>();

        var construction = activities.FirstOrDefault(c => c.NameEn == "Construction");
        if (construction != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "بناء عام", NameEn = "General Construction", ActivityId = construction.Id, IsEnabled = true },
                new Speciality { NameAr = "تصميم داخلي", NameEn = "Interior Design", ActivityId = construction.Id, IsEnabled = true },
                new Speciality { NameAr = "هندسة معمارية", NameEn = "Architecture", ActivityId = construction.Id, IsEnabled = true },
            });
        }

        var realEstate = activities.FirstOrDefault(c => c.NameEn == "Real Estate");
        if (realEstate != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "بيع عقارات", NameEn = "Property Sales", ActivityId = realEstate.Id, IsEnabled = true },
                new Speciality { NameAr = "تأجير عقارات", NameEn = "Property Rental", ActivityId = realEstate.Id, IsEnabled = true },
                new Speciality { NameAr = "إدارة عقارات", NameEn = "Property Management", ActivityId = realEstate.Id, IsEnabled = true },
            });
        }

        var technology = activities.FirstOrDefault(c => c.NameEn == "Technology");
        if (technology != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "تطوير برمجيات", NameEn = "Software Development", ActivityId = technology.Id, IsEnabled = true },
                new Speciality { NameAr = "تصميم مواقع", NameEn = "Web Design", ActivityId = technology.Id, IsEnabled = true },
                new Speciality { NameAr = "دعم تقني", NameEn = "IT Support", ActivityId = technology.Id, IsEnabled = true },
            });
        }

        var maintenance = activities.FirstOrDefault(c => c.NameEn == "Maintenance");
        if (maintenance != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "سباكة", NameEn = "Plumbing", ActivityId = maintenance.Id, IsEnabled = true },
                new Speciality { NameAr = "كهرباء", NameEn = "Electrical", ActivityId = maintenance.Id, IsEnabled = true },
                new Speciality { NameAr = "تكييف وتبريد", NameEn = "HVAC", ActivityId = maintenance.Id, IsEnabled = true },
            });
        }

        var education = activities.FirstOrDefault(c => c.NameEn == "Education");
        if (education != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "تدريب مهني", NameEn = "Professional Training", ActivityId = education.Id, IsEnabled = true },
                new Speciality { NameAr = "تعليم لغات", NameEn = "Language Teaching", ActivityId = education.Id, IsEnabled = true },
                new Speciality { NameAr = "تعليم عن بعد", NameEn = "Online Education", ActivityId = education.Id, IsEnabled = true },
            });
        }

        var healthcare = activities.FirstOrDefault(c => c.NameEn == "Healthcare");
        if (healthcare != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "طب عام", NameEn = "General Medicine", ActivityId = healthcare.Id, IsEnabled = true },
                new Speciality { NameAr = "طب أسنان", NameEn = "Dentistry", ActivityId = healthcare.Id, IsEnabled = true },
                new Speciality { NameAr = "صيدلة", NameEn = "Pharmacy", ActivityId = healthcare.Id, IsEnabled = true },
            });
        }

        var restaurants = activities.FirstOrDefault(c => c.NameEn == "Restaurants & Cafes");
        if (restaurants != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "مطاعم", NameEn = "Restaurants", ActivityId = restaurants.Id, IsEnabled = true },
                new Speciality { NameAr = "مقاهي", NameEn = "Cafes", ActivityId = restaurants.Id, IsEnabled = true },
                new Speciality { NameAr = "تموين وتجهيز", NameEn = "Catering", ActivityId = restaurants.Id, IsEnabled = true },
            });
        }

        var tourism = activities.FirstOrDefault(c => c.NameEn == "Tourism & Travel");
        if (tourism != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "حجز فنادق", NameEn = "Hotel Booking", ActivityId = tourism.Id, IsEnabled = true },
                new Speciality { NameAr = "رحلات سياحية", NameEn = "Tour Packages", ActivityId = tourism.Id, IsEnabled = true },
                new Speciality { NameAr = "تأشيرات", NameEn = "Visa Services", ActivityId = tourism.Id, IsEnabled = true },
            });
        }

        var financial = activities.FirstOrDefault(c => c.NameEn == "Financial Services");
        if (financial != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "محاسبة", NameEn = "Accounting", ActivityId = financial.Id, IsEnabled = true },
                new Speciality { NameAr = "استشارات مالية", NameEn = "Financial Consulting", ActivityId = financial.Id, IsEnabled = true },
                new Speciality { NameAr = "تأمين", NameEn = "Insurance", ActivityId = financial.Id, IsEnabled = true },
            });
        }

        var automotive = activities.FirstOrDefault(c => c.NameEn == "Automotive");
        if (automotive != null)
        {
            specialities.AddRange(new[]
            {
                new Speciality { NameAr = "صيانة سيارات", NameEn = "Car Maintenance", ActivityId = automotive.Id, IsEnabled = true },
                new Speciality { NameAr = "بيع سيارات", NameEn = "Car Sales", ActivityId = automotive.Id, IsEnabled = true },
                new Speciality { NameAr = "قطع غيار", NameEn = "Spare Parts", ActivityId = automotive.Id, IsEnabled = true },
            });
        }

        if (specialities.Count > 0)
        {
            context.Specialities.AddRange(specialities);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPlatformSettingsAsync(QimDbContext context)
    {
        if (await context.PlatformSettings.AnyAsync()) return;

        var settings = new[]
        {
            new PlatformSetting { Key = "SiteName", Value = "QIM", Group = "General" },
            new PlatformSetting { Key = "SiteNameAr", Value = "قيم", Group = "General" },
            new PlatformSetting { Key = "ContactEmail", Value = "info@qim.com", Group = "Contact" },
            new PlatformSetting { Key = "ContactPhone", Value = "+962790000000", Group = "Contact" },
            new PlatformSetting { Key = "FacebookUrl", Value = "https://facebook.com/qim", Group = "Social" },
            new PlatformSetting { Key = "InstagramUrl", Value = "https://instagram.com/qim", Group = "Social" },
            new PlatformSetting { Key = "MaxFileUploadSizeMB", Value = "5", Group = "Upload" },
            new PlatformSetting { Key = "ReviewAutoApprove", Value = "false", Group = "Reviews" },
        };

        context.PlatformSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }

    // ── Egypt: 27 Governorates as Cities ──
    private static async Task SeedEgyptCitiesAsync(QimDbContext context)
    {
        var egypt = await context.Countries.FirstOrDefaultAsync(c => c.NameEn == "Egypt");
        if (egypt == null) return;
        if (await context.Cities.AnyAsync(c => c.CountryId == egypt.Id)) return;

        var governorates = new (string Ar, string En)[]
        {
            ("القاهرة", "Cairo"), ("الجيزة", "Giza"), ("الأسكندرية", "Alexandria"),
            ("الدقهلية", "Dakahlia"), ("البحر الأحمر", "Red Sea"), ("البحيرة", "Beheira"),
            ("الفيوم", "Fayoum"), ("الغربية", "Gharbia"), ("الإسماعيلية", "Ismailia"),
            ("المنوفية", "Menofia"), ("المنيا", "Minya"), ("القليوبية", "Qalyubia"),
            ("الوادي الجديد", "New Valley"), ("السويس", "Suez"), ("أسوان", "Aswan"),
            ("أسيوط", "Assiut"), ("بني سويف", "Beni Suef"), ("بورسعيد", "Port Said"),
            ("دمياط", "Damietta"), ("الشرقية", "Sharkia"), ("جنوب سيناء", "South Sinai"),
            ("كفر الشيخ", "Kafr El Sheikh"), ("مطروح", "Matrouh"), ("الأقصر", "Luxor"),
            ("قنا", "Qena"), ("شمال سيناء", "North Sinai"), ("سوهاج", "Sohag"),
        };

        var cities = governorates.Select(g => new City
        {
            NameAr = g.Ar, NameEn = g.En, CountryId = egypt.Id, IsEnabled = true
        }).ToArray();

        context.Cities.AddRange(cities);
        await context.SaveChangesAsync();
    }

    // ── Egypt: Cities as Districts under their Governorate ──
    private static async Task SeedEgyptDistrictsAsync(QimDbContext context)
    {
        var egypt = await context.Countries.FirstOrDefaultAsync(c => c.NameEn == "Egypt");
        if (egypt == null) return;

        var egyptCities = await context.Cities.Where(c => c.CountryId == egypt.Id).ToListAsync();
        if (!egyptCities.Any()) return;
        // If districts already seeded for first Egypt city, skip
        if (await context.Districts.AnyAsync(d => egyptCities.Select(c => c.Id).Contains(d.CityId))) return;

        // Key cities/districts by governorate
        var districtMap = new Dictionary<string, (string Ar, string En)[]>
        {
            ["Cairo"] = new[] {
                ("مدينة نصر", "Nasr City"), ("المعادي", "Maadi"), ("مصر الجديدة", "Heliopolis"),
                ("الزمالك", "Zamalek"), ("وسط البلد", "Downtown"), ("المقطم", "Mokattam"),
                ("شبرا", "Shubra"), ("حلوان", "Helwan"), ("التجمع الخامس", "Fifth Settlement"),
                ("القاهرة الجديدة", "New Cairo"), ("العباسية", "Abbassia"), ("السيدة زينب", "Sayeda Zeinab"),
                ("الدرب الأحمر", "El Darb El Ahmar"), ("باب الشعرية", "Bab El Sharia"), ("منشأة ناصر", "Manshiet Nasser"),
            },
            ["Giza"] = new[] {
                ("الدقي", "Dokki"), ("المهندسين", "Mohandessin"), ("العجوزة", "Agouza"),
                ("فيصل", "Faisal"), ("الهرم", "Haram"), ("أكتوبر", "October"),
                ("الشيخ زايد", "Sheikh Zayed"), ("بولاق الدكرور", "Bulaq Dakrour"),
                ("إمبابة", "Imbaba"), ("أوسيم", "Ausim"), ("الوراق", "El Warraq"),
                ("كرداسة", "Kerdasa"), ("أبو النمرس", "Abu El Nomros"), ("البدرشين", "Badrshein"),
            },
            ["Alexandria"] = new[] {
                ("سموحة", "Smouha"), ("سيدي بشر", "Sidi Bishr"), ("ستانلي", "Stanley"),
                ("المنتزه", "Montaza"), ("المعمورة", "Maamoura"), ("العصافرة", "El Asafra"),
                ("كليوباترا", "Cleopatra"), ("الإبراهيمية", "Ibrahimia"), ("رشدي", "Rushdi"),
                ("محرم بك", "Moharam Bek"), ("الورديان", "Wardian"), ("العجمي", "El Agami"),
                ("برج العرب", "Borg El Arab"), ("أبو قير", "Abu Qir"),
            },
            ["Dakahlia"] = new[] {
                ("المنصورة", "Mansoura"), ("طلخا", "Talkha"), ("ميت غمر", "Mit Ghamr"),
                ("دكرنس", "Dikirnis"), ("أجا", "Aga"), ("منية النصر", "Minyat El Nasr"),
                ("السنبلاوين", "Sinbellawin"), ("شربين", "Sherbin"), ("بلقاس", "Bilqas"),
                ("نبروه", "Nabaroh"),
            },
            ["Sharkia"] = new[] {
                ("الزقازيق", "Zagazig"), ("العاشر من رمضان", "10th of Ramadan"),
                ("منيا القمح", "Minya El Qamh"), ("بلبيس", "Bilbeis"), ("أبو حماد", "Abu Hammad"),
                ("مشتول السوق", "Mashtool El Souk"), ("أبو كبير", "Abu Kebir"),
                ("فاقوس", "Faqous"), ("الحسينية", "Husseiniya"), ("ههيا", "Hihya"),
            },
            ["Qalyubia"] = new[] {
                ("بنها", "Benha"), ("شبرا الخيمة", "Shubra El Kheima"), ("القناطر الخيرية", "Qanatir"),
                ("الخانكة", "Khanka"), ("قليوب", "Qalyub"), ("طوخ", "Toukh"),
                ("كفر شكر", "Kafr Shukr"), ("العبور", "Obour"),
            },
            ["Gharbia"] = new[] {
                ("طنطا", "Tanta"), ("المحلة الكبرى", "El Mahalla El Kubra"),
                ("كفر الزيات", "Kafr El Zayat"), ("زفتى", "Zefta"), ("السنطة", "Santa"),
                ("قطور", "Qutur"), ("بسيون", "Basyoun"), ("سمنود", "Samannoud"),
            },
            ["Beheira"] = new[] {
                ("دمنهور", "Damanhour"), ("كفر الدوار", "Kafr El Dawwar"), ("رشيد", "Rashid"),
                ("إدكو", "Edku"), ("أبو المطامير", "Abu El Matamir"), ("حوش عيسى", "Hosh Issa"),
                ("شبراخيت", "Shubrakhit"), ("إيتاي البارود", "Itay El Baroud"),
                ("كوم حمادة", "Kom Hamada"), ("وادي النطرون", "Wadi El Natrun"),
            },
            ["Menofia"] = new[] {
                ("شبين الكوم", "Shebin ElKom"), ("منوف", "Menouf"), ("أشمون", "Ashmoun"),
                ("السادات", "Sadat City"), ("تلا", "Tala"), ("الباجور", "El Bagour"),
                ("قويسنا", "Quesna"), ("بركة السبع", "Birket El Sab"),
            },
            ["Minya"] = new[] {
                ("المنيا", "Minya City"), ("ملوي", "Mallawi"), ("بني مزار", "Beni Mazar"),
                ("مغاغة", "Maghagha"), ("سمالوط", "Samalut"), ("أبو قرقاص", "Abu Qurqas"),
                ("دير مواس", "Deir Mawas"), ("العدوة", "El Edwa"), ("مطاي", "Matai"),
            },
            ["Fayoum"] = new[] {
                ("الفيوم", "Fayoum City"), ("إبشواي", "Ibshaway"), ("طامية", "Tamia"),
                ("سنورس", "Sinnuris"), ("إطسا", "Etsa"), ("يوسف الصديق", "Yusuf El Siddiq"),
            },
            ["Assiut"] = new[] {
                ("أسيوط", "Assiut City"), ("ديروط", "Dairut"), ("منفلوط", "Manfalut"),
                ("القوصية", "El Quseyya"), ("أبو تيج", "Abu Tig"), ("الغنايم", "El Ghanayim"),
                ("أبنوب", "Abnoub"), ("ساحل سليم", "Sahel Selim"), ("البداري", "El Badari"),
            },
            ["Aswan"] = new[] {
                ("أسوان", "Aswan City"), ("كوم أمبو", "Kom Ombo"), ("إدفو", "Edfu"),
                ("دراو", "Daraw"), ("نصر النوبة", "Nasr El Nuba"), ("أبو سمبل", "Abu Simbel"),
            },
            ["Luxor"] = new[] {
                ("الأقصر", "Luxor City"), ("الأقصر الجديدة", "New Luxor"), ("إسنا", "Esna"),
                ("أرمنت", "Armant"), ("الزينية", "El Zenia"), ("البياضية", "El Bayadiya"),
            },
            ["Qena"] = new[] {
                ("قنا", "Qena City"), ("نجع حمادي", "Nag Hammadi"), ("قوص", "Qus"),
                ("دشنا", "Dishna"), ("الوقف", "El Waqf"), ("أبو تشت", "Abu Tisht"),
                ("فرشوط", "Farshut"), ("نقادة", "Naqada"),
            },
            ["Sohag"] = new[] {
                ("سوهاج", "Sohag City"), ("أخميم", "Akhmim"), ("طهطا", "Tahta"),
                ("جرجا", "Girga"), ("البلينا", "El Balyana"), ("المنشاة", "El Monsha'a"),
                ("المراغة", "El Maragha"), ("دار السلام", "Dar El Salam"), ("ساقلتة", "Saqulta"),
            },
            ["Beni Suef"] = new[] {
                ("بني سويف", "Beni Suef City"), ("الواسطى", "El Wasta"), ("ناصر", "Nasser"),
                ("إهناسيا", "Ehnasia"), ("ببا", "Beba"), ("الفشن", "El Fashn"), ("سمسطا", "Sumusta"),
            },
            ["Ismailia"] = new[] {
                ("الإسماعيلية", "Ismailia City"), ("القنطرة شرق", "Qantara East"),
                ("القنطرة غرب", "Qantara West"), ("فايد", "Fayed"), ("التل الكبير", "El Tal El Kebir"),
                ("أبو صوير", "Abu Suweir"),
            },
            ["Suez"] = new[] {
                ("السويس", "Suez City"), ("الأربعين", "El Arba'in"), ("عتاقة", "Ataka"),
                ("الجناين", "El Ganayen"), ("فيصل", "Faisal Suez"),
            },
            ["Port Said"] = new[] {
                ("بورسعيد", "Port Said City"), ("بور فؤاد", "Port Fouad"),
                ("العرب", "El Arab"), ("الضواحي", "El Dawahi"), ("المناخ", "El Manakh"), ("الزهور", "El Zohour"),
            },
            ["Damietta"] = new[] {
                ("دمياط", "Damietta City"), ("دمياط الجديدة", "New Damietta"),
                ("رأس البر", "Ras El Bar"), ("فارسكور", "Faraskour"), ("كفر سعد", "Kafr Saad"),
                ("الزرقا", "Ez Zarqa"),
            },
            ["Kafr El Sheikh"] = new[] {
                ("كفر الشيخ", "Kafr El Sheikh City"), ("دسوق", "Desouk"), ("فوه", "Fuwwah"),
                ("بيلا", "Beyla"), ("مطوبس", "Mutubas"), ("قلين", "Qellin"),
                ("الحامول", "El Hamool"), ("سيدي سالم", "Sidi Salem"),
            },
            ["Red Sea"] = new[] {
                ("الغردقة", "Hurghada"), ("سفاجا", "Safaga"), ("مرسى علم", "Marsa Alam"),
                ("القصير", "El Quseir"), ("رأس غارب", "Ras Ghareb"), ("الشلاتين", "Shalatin"),
            },
            ["New Valley"] = new[] {
                ("الخارجة", "Kharga"), ("الداخلة", "Dakhla"), ("الفرافرة", "Farafra"),
                ("باريس", "Paris"), ("بلاط", "Balat"),
            },
            ["Matrouh"] = new[] {
                ("مرسى مطروح", "Marsa Matrouh"), ("الحمام", "El Hammam"), ("العلمين", "El Alamein"),
                ("الضبعة", "Dabaa"), ("النجيلة", "El Negila"), ("سيوة", "Siwa"),
            },
            ["South Sinai"] = new[] {
                ("شرم الشيخ", "Sharm El Sheikh"), ("دهب", "Dahab"), ("نويبع", "Nuweiba"),
                ("طابا", "Taba"), ("سانت كاترين", "Saint Catherine"), ("الطور", "El Tor"),
            },
            ["North Sinai"] = new[] {
                ("العريش", "El Arish"), ("رفح", "Rafah"), ("الشيخ زويد", "Sheikh Zuweid"),
                ("بئر العبد", "Bir El Abd"), ("الحسنة", "El Hasana"), ("نخل", "Nakhl"),
            },
        };

        var districts = new List<District>();
        foreach (var city in egyptCities)
        {
            if (districtMap.TryGetValue(city.NameEn, out var dists))
            {
                foreach (var d in dists)
                    districts.Add(new District { NameAr = d.Ar, NameEn = d.En, CityId = city.Id, IsEnabled = true });
            }
        }

        if (districts.Count > 0)
        {
            context.Districts.AddRange(districts);
            await context.SaveChangesAsync();
        }
    }

    // ── Businesses ──
    private static async Task SeedBusinessesAsync(QimDbContext context)
    {
        if (await context.Businesses.AnyAsync()) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@qim.com");
        var provider1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "provider1@qim.com");
        var provider2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "provider2@qim.com");
        var provider3 = await context.Users.FirstOrDefaultAsync(u => u.Email == "provider3@qim.com");
        var provider4 = await context.Users.FirstOrDefaultAsync(u => u.Email == "provider4@qim.com");
        var provider5 = await context.Users.FirstOrDefaultAsync(u => u.Email == "provider5@qim.com");
        if (admin == null) return;

        var activities = await context.Activities.ToListAsync();
        var specialities = await context.Specialities.ToListAsync();
        var amman = await context.Cities.FirstOrDefaultAsync(c => c.NameEn == "Amman");
        var cairo = await context.Cities.FirstOrDefaultAsync(c => c.NameEn == "Cairo");
        var jordan = await context.Countries.FirstOrDefaultAsync(c => c.NameEn == "Jordan");
        var egypt = await context.Countries.FirstOrDefaultAsync(c => c.NameEn == "Egypt");

        if (amman == null || jordan == null) return;

        var abdali = await context.Districts.FirstOrDefaultAsync(d => d.NameEn == "Abdali");
        var sweifieh = await context.Districts.FirstOrDefaultAsync(d => d.NameEn == "Sweifieh");
        var shmeisani = await context.Districts.FirstOrDefaultAsync(d => d.NameEn == "Shmeisani");
        var nasrCity = cairo != null ? await context.Districts.FirstOrDefaultAsync(d => d.NameEn == "Nasr City") : null;
        var maadi = cairo != null ? await context.Districts.FirstOrDefaultAsync(d => d.NameEn == "Maadi") : null;

        var construction = activities.FirstOrDefault(c => c.NameEn == "Construction");
        var realEstate = activities.FirstOrDefault(c => c.NameEn == "Real Estate");
        var technology = activities.FirstOrDefault(c => c.NameEn == "Technology");
        var education = activities.FirstOrDefault(c => c.NameEn == "Education");
        var healthcare = activities.FirstOrDefault(c => c.NameEn == "Healthcare");
        var restaurants = activities.FirstOrDefault(c => c.NameEn == "Restaurants & Cafes");
        var tourism = activities.FirstOrDefault(c => c.NameEn == "Tourism & Travel");
        var automotive = activities.FirstOrDefault(c => c.NameEn == "Automotive");
        var financial = activities.FirstOrDefault(c => c.NameEn == "Financial Services");
        var maintenance = activities.FirstOrDefault(c => c.NameEn == "Maintenance");

        // Lookup specialities for assignment
        var stGeneralConstruction = specialities.FirstOrDefault(s => s.NameEn == "General Construction");
        var stPropertySales = specialities.FirstOrDefault(s => s.NameEn == "Property Sales");
        var stSoftwareDev = specialities.FirstOrDefault(s => s.NameEn == "Software Development");
        var stProfTraining = specialities.FirstOrDefault(s => s.NameEn == "Professional Training");
        var stGeneralMedicine = specialities.FirstOrDefault(s => s.NameEn == "General Medicine");
        var stRestaurants = specialities.FirstOrDefault(s => s.NameEn == "Restaurants");
        var stTourPackages = specialities.FirstOrDefault(s => s.NameEn == "Tour Packages");
        var stCarMaintenance = specialities.FirstOrDefault(s => s.NameEn == "Car Maintenance");
        var stFinancialConsulting = specialities.FirstOrDefault(s => s.NameEn == "Financial Consulting");
        var stPlumbing = specialities.FirstOrDefault(s => s.NameEn == "Plumbing");

        var businesses = new Business[]
        {
            new() { NameAr = "شركة البناء المتقدمة", NameEn = "Advanced Construction Co.", DescriptionAr = "شركة رائدة في مجال البناء والتشييد في الأردن، نقدم خدمات البناء العام والتصميم الداخلي والإشراف الهندسي بأعلى معايير الجودة والسلامة.", DescriptionEn = "Leading construction company in Jordan offering general construction, interior design, and engineering supervision with the highest quality and safety standards.", OwnerId = (provider1 ?? admin).Id, ActivityId = construction!.Id, SpecialityId = stGeneralConstruction?.Id, Status = BusinessStatus.Approved, IsVerified = true, Rating = 4.5, ReviewCount = 12, Email = "info@advconst.jo", Website = "https://advconst.jo", Phones = "[\"0790001111\",\"0790001112\"]", AccountCode = "" },
            new() { NameAr = "عقارات الأردن", NameEn = "Jordan Real Estate", DescriptionAr = "نقدم خدمات عقارية شاملة تشمل بيع وتأجير وإدارة العقارات السكنية والتجارية في جميع محافظات الأردن.", DescriptionEn = "Comprehensive real estate services including sales, rental, and management of residential and commercial properties across Jordan.", OwnerId = (provider3 ?? admin).Id, ActivityId = realEstate!.Id, SpecialityId = stPropertySales?.Id, Status = BusinessStatus.Approved, IsVerified = true, Rating = 4.2, ReviewCount = 8, Email = "info@jordanre.com", Website = "https://jordanre.com", Phones = "[\"0790002222\",\"0790002223\"]", AccountCode = "" },
            new() { NameAr = "تك سوليوشنز", NameEn = "Tech Solutions", DescriptionAr = "شركة متخصصة في تطوير البرمجيات وتصميم المواقع وتطبيقات الهاتف المحمول، نقدم حلولاً تقنية مبتكرة للشركات والمؤسسات.", DescriptionEn = "Specialized in software development, web design, and mobile applications, delivering innovative technology solutions for businesses and organizations.", OwnerId = (provider2 ?? admin).Id, ActivityId = technology!.Id, SpecialityId = stSoftwareDev?.Id, Status = BusinessStatus.Approved, IsVerified = true, Rating = 4.8, ReviewCount = 25, Email = "hello@techsolutions.jo", Website = "https://techsolutions.jo", Phones = "[\"0790003333\"]", AccountCode = "" },
            new() { NameAr = "أكاديمية النجاح", NameEn = "Success Academy", DescriptionAr = "مركز تدريب وتعليم متكامل يقدم دورات تدريبية مهنية ولغوية معتمدة لتطوير المهارات وبناء القدرات في مختلف المجالات.", DescriptionEn = "Comprehensive training and education center offering certified professional and language courses for skill development and capacity building across various fields.", OwnerId = (provider1 ?? admin).Id, ActivityId = education!.Id, SpecialityId = stProfTraining?.Id, Status = BusinessStatus.Approved, Rating = 4.0, ReviewCount = 6, Email = "learn@successacademy.jo", Website = "https://successacademy.jo", Phones = "[\"0790004444\",\"0790004445\"]", AccountCode = "" },
            new() { NameAr = "مركز الشفاء الطبي", NameEn = "Al-Shifaa Medical Center", DescriptionAr = "مركز طبي متعدد التخصصات يضم نخبة من الأطباء المتخصصين ويقدم خدمات طبية شاملة تشمل الطب العام وطب الأسنان والمختبرات والأشعة.", DescriptionEn = "Multi-specialty medical center with elite specialized doctors, offering comprehensive medical services including general medicine, dentistry, laboratories, and radiology.", OwnerId = (provider4 ?? admin).Id, ActivityId = healthcare!.Id, SpecialityId = stGeneralMedicine?.Id, Status = BusinessStatus.Approved, IsVerified = true, Rating = 4.6, ReviewCount = 18, Email = "info@alshifaa.jo", Website = "https://alshifaa.jo", Phones = "[\"0790005555\",\"0790005556\"]", AccountCode = "" },
            new() { NameAr = "مطعم الديوان", NameEn = "Al-Diwan Restaurant", DescriptionAr = "مطعم متميز يقدم أشهى أطباق المطبخ العربي التقليدي في أجواء شرقية أصيلة، متخصصون في المشاوي والأطباق الشعبية الأردنية.", DescriptionEn = "Distinguished restaurant serving the finest traditional Arabic cuisine in an authentic oriental atmosphere, specializing in grilled dishes and Jordanian folk plates.", OwnerId = (provider3 ?? admin).Id, ActivityId = restaurants!.Id, SpecialityId = stRestaurants?.Id, Status = BusinessStatus.Approved, Rating = 4.3, ReviewCount = 30, Email = "reserve@aldiwan.jo", Website = "https://aldiwan.jo", Phones = "[\"0790006666\"]", AccountCode = "" },
            new() { NameAr = "رحلات الأردن", NameEn = "Jordan Trips", DescriptionAr = "وكالة سياحة وسفر متكاملة توفر حجوزات الفنادق والرحلات السياحية الداخلية والخارجية وخدمات التأشيرات بأسعار تنافسية.", DescriptionEn = "Full-service tourism and travel agency providing hotel bookings, domestic and international tour packages, and visa services at competitive prices.", OwnerId = (provider2 ?? admin).Id, ActivityId = tourism!.Id, SpecialityId = stTourPackages?.Id, Status = BusinessStatus.Approved, Rating = 4.1, ReviewCount = 10, Email = "info@jordantrips.jo", Website = "https://jordantrips.jo", Phones = "[\"0790007777\",\"0790007778\"]", AccountCode = "" },
            new() { NameAr = "أوتو سيرفس", NameEn = "Auto Service Center", DescriptionAr = "مركز خدمة متكامل للسيارات يقدم خدمات الصيانة الدورية والإصلاح والفحص الشامل وبيع قطع الغيار الأصلية لجميع أنواع السيارات.", DescriptionEn = "Comprehensive auto service center providing periodic maintenance, repairs, full inspections, and genuine spare parts for all vehicle types.", OwnerId = (provider5 ?? admin).Id, ActivityId = automotive!.Id, SpecialityId = stCarMaintenance?.Id, Status = BusinessStatus.Approved, Rating = 3.9, ReviewCount = 15, Email = "service@autoservice.jo", Phones = "[\"0790008888\"]", AccountCode = "" },
            new() { NameAr = "الأمان للخدمات المالية", NameEn = "Al-Aman Financial", DescriptionAr = "شركة استشارات مالية ومحاسبية متخصصة تقدم خدمات التدقيق والمحاسبة والاستشارات الضريبية وإعداد الميزانيات للشركات والأفراد.", DescriptionEn = "Specialized financial and accounting consultancy offering audit, accounting, tax advisory, and budgeting services for businesses and individuals.", OwnerId = (provider4 ?? admin).Id, ActivityId = financial!.Id, SpecialityId = stFinancialConsulting?.Id, Status = BusinessStatus.Pending, Rating = 0, ReviewCount = 0, Email = "finance@alaman.jo", Website = "https://alaman.jo", Phones = "[\"0790009999\"]", AccountCode = "" },
            new() { NameAr = "صيانة البيت", NameEn = "Home Fix", DescriptionAr = "شركة متخصصة في خدمات صيانة وإصلاح المنازل تشمل السباكة والكهرباء والتكييف والدهان والنجارة بفريق فني محترف.", DescriptionEn = "Specialized home maintenance and repair company covering plumbing, electrical, HVAC, painting, and carpentry with a professional technical team.", OwnerId = (provider5 ?? admin).Id, ActivityId = maintenance!.Id, SpecialityId = stPlumbing?.Id, Status = BusinessStatus.Approved, Rating = 4.4, ReviewCount = 20, Email = "fix@homefix.jo", Phones = "[\"0790010000\",\"0790010001\"]", AccountCode = "" },
            new() { NameAr = "شركة العمران", NameEn = "Al-Omran Company", DescriptionAr = "شركة مقاولات عامة متخصصة في البناء والتشييد والتطوير العقاري، تنفذ مشاريع سكنية وتجارية بمعايير هندسية عالية.", DescriptionEn = "General contracting company specializing in construction, building, and real estate development, executing residential and commercial projects with high engineering standards.", OwnerId = (provider1 ?? admin).Id, ActivityId = construction.Id, SpecialityId = stGeneralConstruction?.Id, Status = BusinessStatus.Rejected, RejectionReason = "Incomplete documentation", Rating = 0, ReviewCount = 0, Phones = "[\"0790011111\"]", AccountCode = "" },
            new() { NameAr = "مطعم السلطان", NameEn = "Sultan Restaurant", DescriptionAr = "مطعم راقي يقدم تجربة طعام فاخرة مع قائمة متنوعة من الأطباق العربية والعالمية في أجواء أنيقة ومميزة.", DescriptionEn = "Upscale restaurant offering a fine dining experience with a diverse menu of Arabic and international dishes in an elegant and distinguished atmosphere.", OwnerId = (provider3 ?? admin).Id, ActivityId = restaurants.Id, SpecialityId = stRestaurants?.Id, Status = BusinessStatus.Suspended, Rating = 3.5, ReviewCount = 5, Email = "info@sultan.jo", Phones = "[\"0790012222\"]", AccountCode = "" },
        };

        // Set AccountCode to first phone number for each business
        foreach (var biz in businesses)
        {
            if (!string.IsNullOrWhiteSpace(biz.Phones))
            {
                var raw = biz.Phones.Trim();
                if (raw.StartsWith("["))
                {
                    var arr = System.Text.Json.JsonSerializer.Deserialize<string[]>(raw);
                    biz.AccountCode = arr?.FirstOrDefault()?.Trim() ?? "";
                }
                else
                {
                    biz.AccountCode = raw.Split(',')[0].Trim();
                }
            }
        }

        context.Businesses.AddRange(businesses);
        await context.SaveChangesAsync();

        // Seed addresses
        var savedBusinesses = await context.Businesses.ToListAsync();
        var addresses = new List<BusinessAddress>();
        var i = 0;
        foreach (var biz in savedBusinesses)
        {
            var isEgypt = i >= 8 && cairo != null && egypt != null;
            addresses.Add(new BusinessAddress
            {
                BusinessId = biz.Id,
                CountryId = isEgypt ? egypt!.Id : jordan!.Id,
                CityId = isEgypt ? cairo!.Id : amman.Id,
                DistrictId = i switch
                {
                    0 => abdali?.Id,
                    1 => sweifieh?.Id,
                    2 => shmeisani?.Id,
                    3 => abdali?.Id,
                    4 => sweifieh?.Id,
                    5 => shmeisani?.Id,
                    8 => nasrCity?.Id,
                    9 => maadi?.Id,
                    _ => null
                },
                StreetName = $"Street {i + 1}",
                BuildingNumber = $"{(i + 1) * 10}",
                Latitude = 31.95 + (i * 0.01),
                Longitude = 35.93 + (i * 0.01),
                IsPrimary = true,
            });
            i++;
        }
        context.BusinessAddresses.AddRange(addresses);
        await context.SaveChangesAsync();

        // Seed work hours for first 6 businesses
        var workHours = new List<BusinessWorkHours>();
        foreach (var biz in savedBusinesses.Take(6))
        {
            for (var day = DayOfWeek.Sunday; day <= DayOfWeek.Thursday; day++)
            {
                workHours.Add(new BusinessWorkHours
                {
                    BusinessId = biz.Id,
                    DayOfWeek = day,
                    OpenTime = new TimeSpan(9, 0, 0),
                    CloseTime = new TimeSpan(18, 0, 0),
                    IsClosed = false
                });
            }
            workHours.Add(new BusinessWorkHours { BusinessId = biz.Id, DayOfWeek = DayOfWeek.Friday, IsClosed = true });
            workHours.Add(new BusinessWorkHours { BusinessId = biz.Id, DayOfWeek = DayOfWeek.Saturday, OpenTime = new TimeSpan(10, 0, 0), CloseTime = new TimeSpan(14, 0, 0), IsClosed = false });
        }
        context.BusinessWorkHours.AddRange(workHours);
        await context.SaveChangesAsync();
    }

    // ── Business Keywords ──
    private static async Task SeedKeywordsAsync(QimDbContext context)
    {
        if (await context.BusinessKeywords.AnyAsync()) return;

        var businesses = await context.Businesses.ToListAsync();
        if (!businesses.Any()) return;

        var keywordMap = new Dictionary<string, string[]>
        {
            ["Advanced Construction Co."] = new[] { "construction", "building", "concrete", "contractor", "مقاولات", "بناء" },
            ["Jordan Real Estate"] = new[] { "real estate", "property", "apartments", "rent", "عقارات", "شقق" },
            ["Tech Solutions"] = new[] { "software", "IT", "web development", "programming", "برمجيات", "تطوير" },
            ["Success Academy"] = new[] { "education", "training", "courses", "learning", "تدريب", "تعليم" },
            ["Al-Shifaa Medical Center"] = new[] { "medical", "health", "clinic", "doctor", "طبي", "عيادة" },
            ["Al-Diwan Restaurant"] = new[] { "food", "restaurant", "arabic cuisine", "dining", "مطعم", "طعام" },
            ["Jordan Trips"] = new[] { "tourism", "travel", "tours", "vacation", "سياحة", "سفر" },
            ["Auto Service Center"] = new[] { "automotive", "car repair", "mechanic", "maintenance", "سيارات", "صيانة" },
            ["Al-Aman Financial"] = new[] { "finance", "accounting", "consulting", "tax", "مالية", "محاسبة" },
            ["Home Fix"] = new[] { "maintenance", "plumbing", "electrical", "repair", "صيانة", "إصلاح" },
            ["Al-Omran Company"] = new[] { "construction", "contracting", "building", "مقاولات", "بناء" },
            ["Sultan Restaurant"] = new[] { "food", "fine dining", "restaurant", "مطعم", "طعام" },
        };

        var keywords = new List<BusinessKeyword>();
        foreach (var biz in businesses)
        {
            if (keywordMap.TryGetValue(biz.NameEn, out var kws))
            {
                foreach (var kw in kws)
                    keywords.Add(new BusinessKeyword { BusinessId = biz.Id, Keyword = kw });
            }
        }

        if (keywords.Count > 0)
        {
            context.BusinessKeywords.AddRange(keywords);
            await context.SaveChangesAsync();
        }
    }

    // ── Reviews ──
    private static async Task SeedReviewsAsync(QimDbContext context)
    {
        if (await context.Reviews.AnyAsync()) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@qim.com");
        if (admin == null) return;

        var businesses = await context.Businesses.Where(b => b.Status == BusinessStatus.Approved).Take(6).ToListAsync();
        if (!businesses.Any()) return;

        var reviews = new List<Review>();
        var comments = new[]
        {
            ("خدمة ممتازة", "Excellent service, highly recommended!"),
            ("جيد جداً", "Very good experience overall."),
            ("خدمة عادية", "Average service, could improve."),
            ("رائع", "Outstanding quality and professional staff."),
            ("جيد", "Good value for money."),
        };

        foreach (var biz in businesses)
        {
            for (var j = 0; j < 5; j++)
            {
                reviews.Add(new Review
                {
                    BusinessId = biz.Id,
                    UserId = admin.Id,
                    Rating = 3 + (j % 3),
                    Comment = comments[j % comments.Length].Item2,
                    Status = j < 4 ? ReviewStatus.Approved : ReviewStatus.Pending,
                });
            }
        }

        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }

    // ── Blog Posts ──
    private static async Task SeedBlogPostsAsync(QimDbContext context)
    {
        if (await context.BlogPosts.AnyAsync()) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@qim.com");
        if (admin == null) return;

        var posts = new BlogPost[]
        {
            new() { TitleAr = "كيف تختار شركة مقاولات موثوقة", TitleEn = "How to Choose a Reliable Construction Company", ContentAr = "عند البحث عن شركة مقاولات موثوقة، هناك عدة عوامل يجب مراعاتها مثل الخبرة والسمعة والتراخيص.", ContentEn = "When looking for a reliable construction company, consider factors such as experience, reputation, and proper licensing.", Excerpt = "Tips for choosing the right construction company", Category = "Construction", AuthorId = admin.Id, Status = BlogPostStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-30) },
            new() { TitleAr = "أهم النصائح لشراء عقار في الأردن", TitleEn = "Top Tips for Buying Property in Jordan", ContentAr = "سوق العقارات في الأردن يشهد نمواً مستمراً. إليك أهم النصائح لشراء عقار مناسب.", ContentEn = "Jordan's real estate market continues to grow. Here are the top tips for buying the right property.", Excerpt = "Essential guide for property buyers in Jordan", Category = "Real Estate", AuthorId = admin.Id, Status = BlogPostStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-20) },
            new() { TitleAr = "مستقبل التكنولوجيا في المنطقة", TitleEn = "The Future of Technology in the Region", ContentAr = "تشهد المنطقة العربية تحولاً رقمياً كبيراً مع تزايد الاعتماد على التقنيات الحديثة.", ContentEn = "The Arab region is witnessing a major digital transformation with increasing reliance on modern technologies.", Excerpt = "Digital transformation in the Middle East", Category = "Technology", AuthorId = admin.Id, Status = BlogPostStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-10) },
            new() { TitleAr = "أفضل المطاعم العربية في عمان", TitleEn = "Best Arabic Restaurants in Amman", ContentAr = "عمان تضم مجموعة رائعة من المطاعم التي تقدم أشهى الأطباق العربية التقليدية.", ContentEn = "Amman hosts a wonderful collection of restaurants serving the finest traditional Arabic dishes.", Excerpt = "Top dining spots in Amman", Category = "Restaurants", AuthorId = admin.Id, Status = BlogPostStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-5) },
            new() { TitleAr = "دليل السياحة في البتراء", TitleEn = "Petra Tourism Guide", ContentAr = "البتراء، المدينة الوردية، من أهم المعالم السياحية في العالم. تعرف على أفضل الأوقات لزيارتها.", ContentEn = "Petra, the Rose City, is one of the world's most important tourist landmarks. Learn about the best times to visit.", Excerpt = "Complete guide to visiting Petra", Category = "Tourism", AuthorId = admin.Id, Status = BlogPostStatus.Published, PublishedAt = DateTime.UtcNow.AddDays(-2) },
            new() { TitleAr = "نصائح لصيانة المنزل", TitleEn = "Home Maintenance Tips", ContentAr = "الصيانة الدورية للمنزل تحافظ على قيمة العقار وتوفر التكاليف على المدى البعيد.", ContentEn = "Regular home maintenance preserves property value and saves costs in the long run.", Category = "Maintenance", AuthorId = admin.Id, Status = BlogPostStatus.Draft },
        };

        context.BlogPosts.AddRange(posts);
        await context.SaveChangesAsync();
    }

    // ── Business Claims ──
    private static async Task SeedClaimsAsync(QimDbContext context)
    {
        if (await context.BusinessClaims.AnyAsync()) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@qim.com");
        var businesses = await context.Businesses.Take(3).ToListAsync();
        if (admin == null || !businesses.Any()) return;

        var claims = new BusinessClaim[]
        {
            new() { BusinessId = businesses[0].Id, UserId = admin.Id, Phone = "0791111111", Email = "claim1@test.com", Message = "I am the owner of this business.", Status = ClaimStatus.Pending },
            new() { BusinessId = businesses[1].Id, UserId = admin.Id, Phone = "0792222222", Email = "claim2@test.com", Message = "This is my business, please verify.", Status = ClaimStatus.Approved },
            new() { BusinessId = businesses[2].Id, UserId = admin.Id, Phone = "0793333333", Message = "Claiming ownership of this business.", Status = ClaimStatus.Rejected },
        };

        context.BusinessClaims.AddRange(claims);
        await context.SaveChangesAsync();
    }

    // ── Contact Requests ──
    private static async Task SeedContactsAsync(QimDbContext context)
    {
        if (await context.ContactRequests.AnyAsync()) return;

        var contacts = new ContactRequest[]
        {
            new() { Name = "أحمد محمد", Phone = "0791234567", Email = "ahmed@example.com", Message = "أريد الاستفسار عن طريقة التسجيل في المنصة.", Status = ContactStatus.New },
            new() { Name = "Sara Ali", Phone = "0797654321", Email = "sara@example.com", Message = "I have a question about listing my business.", Status = ContactStatus.InProgress, AdminNotes = "Contacted via email" },
            new() { Name = "محمد خالد", Email = "mk@example.com", Message = "اقتراح لتحسين واجهة المستخدم.", Status = ContactStatus.Resolved, AdminNotes = "Forwarded to development team" },
            new() { Name = "Layla Hassan", Phone = "0799988877", Message = "How can I advertise on the platform?", Status = ContactStatus.New },
            new() { Name = "عمر يوسف", Phone = "0798765432", Email = "omar@example.com", Message = "شكراً على الخدمة الرائعة!", Status = ContactStatus.Resolved },
        };

        context.ContactRequests.AddRange(contacts);
        await context.SaveChangesAsync();
    }

    // ── Suggestions ──
    private static async Task SeedSuggestionsAsync(QimDbContext context)
    {
        if (await context.Suggestions.AnyAsync()) return;

        var suggestions = new Suggestion[]
        {
            new() { Name = "خالد عبدالله", Phone = "0791112233", Email = "khaled@example.com", Message = "أقترح إضافة ميزة المقارنة بين الشركات.", Status = SuggestionStatus.New },
            new() { Name = "Noor Ahmad", Email = "noor@example.com", Message = "It would be great to add a mobile app.", Status = SuggestionStatus.Reviewed },
            new() { Name = "سامي حسن", Phone = "0794455667", Message = "أتمنى إضافة خاصية التقييم بالصور.", Status = SuggestionStatus.New },
        };

        context.Suggestions.AddRange(suggestions);
        await context.SaveChangesAsync();
    }

    // ── Advertisements ──
    private static async Task SeedAdvertisementsAsync(QimDbContext context)
    {
        if (await context.Advertisements.AnyAsync()) return;

        var ads = new Advertisement[]
        {
            new() { TitleAr = "إعلان الصفحة الرئيسية", TitleEn = "Homepage Banner Ad", ImageUrl = "/images/ads/banner1.jpg", TargetUrl = "https://example.com/promo1", Position = "homepage_top", IsActive = true, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(60) },
            new() { TitleAr = "إعلان صفحة البحث", TitleEn = "Search Page Sidebar Ad", ImageUrl = "/images/ads/sidebar1.jpg", TargetUrl = "https://example.com/promo2", Position = "search_sidebar", IsActive = true, StartDate = DateTime.UtcNow.AddDays(-15), EndDate = DateTime.UtcNow.AddDays(45) },
        };

        context.Advertisements.AddRange(ads);
        await context.SaveChangesAsync();
    }
}
