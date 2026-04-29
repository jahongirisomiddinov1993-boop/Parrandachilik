using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Printing;

namespace Parrandachilik
{
    // ================= MODELLAR =================
    public class ChekMahsulotModel : INotifyPropertyChanged
    {
        private double _narx, _soni;
        public string Mahsulot { get; set; }
        public double Narx { get => _narx; set { _narx = value; OnPropertyChanged("Narx"); OnPropertyChanged("Summa"); } }
        public double Soni { get => _soni; set { _soni = value; OnPropertyChanged("Soni"); OnPropertyChanged("Summa"); } }
        public double Bonus { get; set; }
        public double Summa => Narx * Soni;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class OmborHolatiModel { public string Mahsulot { get; set; } public double BoshQoldiq { get; set; } public double Terildi { get; set; } public double Sotildi { get; set; } public double YakuniyQoldiq => BoshQoldiq + Terildi - Sotildi; }
    public class MijozModel { public string Nomi { get; set; } public string Tel { get; set; } public string Manzil { get; set; } public string ShartnomaSana { get; set; } public string ShartnomaRaqami { get; set; } }
    public class PudratchiModel { public string Nomi { get; set; } public string Tel { get; set; } public string Manzil { get; set; } public string ShartnomaSana { get; set; } public string ShartnomaRaqami { get; set; } }
    public class HodimModel { public string FISH { get; set; } public string Tel { get; set; } public string ShartnomaSana { get; set; } }
    public class OddiyKatalogModel { public string Nomi { get; set; } }
    public class XaridModel
    {
        public string Sana { get; set; }
        public string Nomi { get; set; }
        public double Miqdori { get; set; }
        public string Valyuta { get; set; }
        public double Narx { get; set; }
        public double Kurs { get; set; }
        public double JamiUzs { get; set; }
        public string Izoh { get; set; }
    }
    public class AktSverkaModel
    {
        public string Sana { get; set; }
        public string Hujjat { get; set; }
        public double Kirim { get; set; }
        public double Chiqim { get; set; }
        public double Saldo { get; set; }
        public string Izoh { get; set; }
    }
    public class ChekModel
    {
        public int ID { get; set; }
        public string Mijoz { get; set; }
        public double Summa { get; set; }
        public string Sana { get; set; }
        public double Naqd { get; set; }
        public double Karta { get; set; }
        public double Kochirilgan { get; set; }
        public double EskiHaq { get; set; }
        public double BuHaq { get; set; }
        public bool HolatYashil { get; set; }
        public string HolatBelgi => HolatYashil ? "✓" : "○";
        public string SotuvHolati { get; set; } = "Sariq";
        public string OmborHolati { get; set; } = "Kutilmoqda";
        public List<ChekItemSnapshotModel> Items { get; set; } = new List<ChekItemSnapshotModel>();
    }
    public class ChekItemSnapshotModel
    {
        public string Mahsulot { get; set; }
        public double Narx { get; set; }
        public double Soni { get; set; }
        public double Summa { get; set; }
    }
    public class SexModel
    {
        public string Nomi { get; set; }
    }
    public class YarusModel
    {
        public string SexNomi { get; set; }
        public string Nomi { get; set; }
    }
    public class OmborHarakatModel
    {
        public DateTime Sana { get; set; }
        public string Mahsulot { get; set; }
        public double Miqdor { get; set; }
    }
    public class KassaKirimModel
    {
        public int ChekID { get; set; }
        public string Sana { get; set; }
        public string Mijoz { get; set; }
        public double Naqd { get; set; }
        public string Holat { get; set; } = "Kutilmoqda";
    }
    public class KassaRashodModel
    {
        public string Raqam { get; set; }
        public string Sana { get; set; }
        public string Turi { get; set; }
        public string Kimga { get; set; }
        public string Izoh { get; set; }
        public double Summa { get; set; }
    }
    public class KassaKimgaOption
    {
        public string Nomi { get; set; }
        public string Manba { get; set; }
    }
    public class JujaKirimModel
    {
        public string Sana { get; set; }
        public string SexNomi { get; set; }
        public string YarusNomi { get; set; }
        public double KelganSoni { get; set; }
        public double KasalSoni { get; set; }
        public double NobudSoni { get; set; }
        public string Izoh { get; set; }
        public double SoglomQoldi => Math.Max(0, KelganSoni - KasalSoni - NobudSoni);
    }

    public partial class MainWindow : Window
    {
        private class UiTextEntry
        {
            public string Base { get; set; }
            public string Uz { get; set; }
            public string Ru { get; set; }
        }

        private readonly List<UiTextEntry> uiTexts = new List<UiTextEntry>
        {
            new UiTextEntry { Base = "Parrandachilik Buxgalteriyasi", Uz = "Паррандачилик бухгалтерияси", Ru = "Бухгалтерия птицеводства" },
            new UiTextEntry { Base = "🏠 Asosiy", Uz = "🏠 Асосий", Ru = "🏠 Главная" },
            new UiTextEntry { Base = "🏭 Ishlab chiqarish", Uz = "🏭 Ишлаб чиқариш", Ru = "🏭 Производство" },
            new UiTextEntry { Base = "🌾 Yem Ombor", Uz = "🌾 Ем Омбор", Ru = "🌾 Склад корма" },
            new UiTextEntry { Base = "🥚 Tuxum Ombor", Uz = "🥚 Тухум Омбор", Ru = "🥚 Склад яиц" },
            new UiTextEntry { Base = "🐔 Parranda", Uz = "🐔 Парранда", Ru = "🐔 Птица" },
            new UiTextEntry { Base = "💩 Gung", Uz = "💩 Гўнг", Ru = "💩 Помет" },
            new UiTextEntry { Base = "💰 Kassa", Uz = "💰 Касса", Ru = "💰 Касса" },
            new UiTextEntry { Base = "🛍️ Sotish", Uz = "🛍️ Сотиш", Ru = "🛍️ Продажа" },
            new UiTextEntry { Base = "🛒 Xarid", Uz = "🛒 Харид", Ru = "🛒 Закупка" },
            new UiTextEntry { Base = "💸 Ish haqi", Uz = "💸 Иш ҳақи", Ru = "💸 Зарплата" },
            new UiTextEntry { Base = "📊 Hisobot", Uz = "📊 Ҳисобот", Ru = "📊 Отчет" },
            new UiTextEntry { Base = "📈 Statistika", Uz = "📈 Статистика", Ru = "📈 Статистика" },
            new UiTextEntry { Base = "⚙️ Sozlamalar", Uz = "⚙️ Созламалар", Ru = "⚙️ Настройки" },
            new UiTextEntry { Base = "Asosiy", Uz = "Асосий", Ru = "Главная" },
            new UiTextEntry { Base = "Ishlab chiqarish", Uz = "Ишлаб чиқариш", Ru = "Производство" },
            new UiTextEntry { Base = "Yem Ombor", Uz = "Ем Омбор", Ru = "Склад корма" },
            new UiTextEntry { Base = "Tuxum Ombor", Uz = "Тухум Омбор", Ru = "Склад яиц" },
            new UiTextEntry { Base = "Parranda", Uz = "Парранда", Ru = "Птица" },
            new UiTextEntry { Base = "Gung", Uz = "Гўнг", Ru = "Помет" },
            new UiTextEntry { Base = "Kassa", Uz = "Касса", Ru = "Касса" },
            new UiTextEntry { Base = "Sotish", Uz = "Сотиш", Ru = "Продажа" },
            new UiTextEntry { Base = "Xarid", Uz = "Харид", Ru = "Закупка" },
            new UiTextEntry { Base = "Ish haqi", Uz = "Иш ҳақи", Ru = "Зарплата" },
            new UiTextEntry { Base = "Hisobot", Uz = "Ҳисобот", Ru = "Отчет" },
            new UiTextEntry { Base = "Statistika", Uz = "Статистика", Ru = "Статистика" },
            new UiTextEntry { Base = "⚙️ Sozlamalar", Uz = "⚙️ Созламалар", Ru = "⚙️ Настройки" },
            new UiTextEntry { Base = "Kunlik kassa holati", Uz = "Кунлик касса ҳолати", Ru = "Состояние кассы за день" },
            new UiTextEntry { Base = "Kirim (Sotuvdan naqd tushum)", Uz = "Кирим (Сотувдан нақд тушум)", Ru = "Приход (наличные продажи)" },
            new UiTextEntry { Base = "Rashodlar", Uz = "Расходлар", Ru = "Расходы" },
            new UiTextEntry { Base = "✅ Kassa qabul qildi", Uz = "✅ Касса қабул қилди", Ru = "✅ Касса приняла" },
            new UiTextEntry { Base = "+ Qo'shish", Uz = "+ Қўшиш", Ru = "+ Добавить" },
            new UiTextEntry { Base = "🗑 O'chirish", Uz = "🗑 Ўчириш", Ru = "🗑 Удалить" },
            new UiTextEntry { Base = "🖨 Print", Uz = "🖨 Чоп этиш", Ru = "🖨 Печать" },
            new UiTextEntry { Base = "Harajat turi", Uz = "Харажат тури", Ru = "Вид расхода" },
            new UiTextEntry { Base = "Kimga", Uz = "Кимга", Ru = "Кому" },
            new UiTextEntry { Base = "To'lov tafsiloti", Uz = "Тўлов тафсилоти", Ru = "Детали платежа" },
            new UiTextEntry { Base = "Summa", Uz = "Сумма", Ru = "Сумма" },
            new UiTextEntry { Base = "Sana", Uz = "Сана", Ru = "Дата" },
            new UiTextEntry { Base = "Kun boshi:", Uz = "Кун боши:", Ru = "Начало дня:" },
            new UiTextEntry { Base = "Kirim:", Uz = "Кирим:", Ru = "Приход:" },
            new UiTextEntry { Base = "Rashod:", Uz = "Расход:", Ru = "Расход:" },
            new UiTextEntry { Base = "Kun oxiri:", Uz = "Кун охири:", Ru = "Конец дня:" },
            new UiTextEntry { Base = "Qidirish:", Uz = "Қидириш:", Ru = "Поиск:" },
            new UiTextEntry { Base = "Nomi", Uz = "Номи", Ru = "Наименование" },
            new UiTextEntry { Base = "Manba", Uz = "Манба", Ru = "Источник" },
            new UiTextEntry { Base = "Tuxum kiritish", Uz = "Тухум киритиш", Ru = "Ввод яйца" },
            new UiTextEntry { Base = "Juja sexidan kelim", Uz = "Жўжа цехидан келим", Ru = "Приход из цеха цыплят" },
            new UiTextEntry { Base = "Kelgan", Uz = "Келган", Ru = "Поступило" },
            new UiTextEntry { Base = "Kasal", Uz = "Касал", Ru = "Больные" },
            new UiTextEntry { Base = "Nobud", Uz = "Нобуд", Ru = "Падеж" },
            new UiTextEntry { Base = "Sog'lom", Uz = "Соғлом", Ru = "Здоровые" },
            new UiTextEntry { Base = "Izoh:", Uz = "Изоҳ:", Ru = "Комментарий:" },
            new UiTextEntry { Base = "➕ Saqlash", Uz = "➕ Сақлаш", Ru = "➕ Сохранить" }
        };

        ObservableCollection<ChekMahsulotModel> ChekMahsulotlar = new ObservableCollection<ChekMahsulotModel>();
        ObservableCollection<OmborHolatiModel> OmborHolat = new ObservableCollection<OmborHolatiModel>();
        ObservableCollection<MijozModel> BazaMijozlar = new ObservableCollection<MijozModel>();
        ObservableCollection<PudratchiModel> BazaPudratchilar = new ObservableCollection<PudratchiModel>();
        ObservableCollection<HodimModel> BazaHodimlar = new ObservableCollection<HodimModel>();
        ObservableCollection<OddiyKatalogModel> BazaHomAshyo = new ObservableCollection<OddiyKatalogModel>();
        ObservableCollection<OddiyKatalogModel> BazaTayyorMahsulot = new ObservableCollection<OddiyKatalogModel>();
        ObservableCollection<OddiyKatalogModel> BazaOlchovlar = new ObservableCollection<OddiyKatalogModel>();
        ObservableCollection<XaridModel> Xaridlar = new ObservableCollection<XaridModel>();
        ObservableCollection<AktSverkaModel> AktMijozItems = new ObservableCollection<AktSverkaModel>();
        ObservableCollection<AktSverkaModel> AktPudratchiItems = new ObservableCollection<AktSverkaModel>();
        ObservableCollection<TuxumModel> IshlabChiqarishlar = new ObservableCollection<TuxumModel>();
        ObservableCollection<TuxumModel> BlockedPartiyalar = new ObservableCollection<TuxumModel>();
        ObservableCollection<ChekModel> Cheklar = new ObservableCollection<ChekModel>();
        ObservableCollection<SexModel> Sexlar = new ObservableCollection<SexModel>();
        ObservableCollection<YarusModel> Yaruslar = new ObservableCollection<YarusModel>();
        ObservableCollection<YarusModel> IshlabYaruslar = new ObservableCollection<YarusModel>();
        ObservableCollection<OmborHarakatModel> KirimHarakatlar = new ObservableCollection<OmborHarakatModel>();
        ObservableCollection<OmborHarakatModel> SotuvHarakatlar = new ObservableCollection<OmborHarakatModel>();
        ObservableCollection<KassaKirimModel> KassaKirimlar = new ObservableCollection<KassaKirimModel>();
        ObservableCollection<KassaRashodModel> KassaRashodlar = new ObservableCollection<KassaRashodModel>();
        ObservableCollection<string> KassaRashodTurlari = new ObservableCollection<string>();
        ObservableCollection<KassaKimgaOption> KassaKimgaOptions = new ObservableCollection<KassaKimgaOption>();
        ObservableCollection<YarusModel> JujaYaruslar = new ObservableCollection<YarusModel>();
        ObservableCollection<JujaKirimModel> JujaKirimlar = new ObservableCollection<JujaKirimModel>();
        ICollectionView kassaKirimView;
        ICollectionView popupMijozlarView;
        ICollectionView popupKassaKimgaView;
        ICollectionView omborView;
        double milliyBankUsdKursi = 12750;
        string currentLanguage = "uz";
        readonly string languageStatePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Parrandachilik",
            "language.txt");

        public MainWindow()
        {
            InitializeComponent();
            string bugun = DateTime.Now.ToString("dd.MM.yyyy");
            if (txtMaxsusSana != null) txtMaxsusSana.Text = bugun;
            if (txtSanaFiltr != null) txtSanaFiltr.Text = bugun;
            if (dpChekSanaFiltr != null) dpChekSanaFiltr.SelectedDate = DateTime.Now;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] kategoriyalar = { "1.6 kg", "1.7 kg", "1.8 kg", "1.9 kg", "2.0 kg", "2.5 kg", "Siniq", "Paket", "Nuri" };
            foreach (string kat in kategoriyalar)
            {
                ChekMahsulotlar.Add(new ChekMahsulotModel { Mahsulot = kat, Narx = 0, Soni = 0, Bonus = 0 });
                OmborHolat.Add(new OmborHolatiModel { Mahsulot = kat, BoshQoldiq = 0, Terildi = 0, Sotildi = 0 });
            }
            if (dgChekMahsulot != null) dgChekMahsulot.ItemsSource = ChekMahsulotlar;
            if (dgSotuvOmborHolati != null) dgSotuvOmborHolati.ItemsSource = OmborHolat;
            if (dgIshlabChiqarish != null) dgIshlabChiqarish.ItemsSource = IshlabChiqarishlar;
            if (dgOmbor != null) dgOmbor.ItemsSource = IshlabChiqarishlar;
            if (dgCheklar != null) dgCheklar.ItemsSource = Cheklar;
            if (dgKassaKirim != null) dgKassaKirim.ItemsSource = KassaKirimlar;
            if (dgKassaRashod != null) dgKassaRashod.ItemsSource = KassaRashodlar;
            if (dgJujaKirimlar != null) dgJujaKirimlar.ItemsSource = JujaKirimlar;
            if (cmbKassaRashodTuri != null) cmbKassaRashodTuri.ItemsSource = KassaRashodTurlari;
            if (dgPopupKassaKimga != null) dgPopupKassaKimga.ItemsSource = KassaKimgaOptions;
            if (dgBlockedPartiyalar != null) dgBlockedPartiyalar.ItemsSource = BlockedPartiyalar;
            if (dgSozlamaSexlar != null) dgSozlamaSexlar.ItemsSource = Sexlar;
            if (dgSozlamaYaruslar != null) dgSozlamaYaruslar.ItemsSource = Yaruslar;
            if (cmbYarusSexTanlash != null) cmbYarusSexTanlash.ItemsSource = Sexlar;
            if (cmbIshlabSex != null) cmbIshlabSex.ItemsSource = Sexlar;
            if (cmbIshlabYarus != null) cmbIshlabYarus.ItemsSource = IshlabYaruslar;
            if (cmbIshlabHodim != null) cmbIshlabHodim.ItemsSource = BazaHodimlar;
            if (cmbJujaSex != null) cmbJujaSex.ItemsSource = Sexlar;
            if (cmbJujaYarus != null) cmbJujaYarus.ItemsSource = JujaYaruslar;

            if (dgSozlamaMijozlar != null) dgSozlamaMijozlar.ItemsSource = BazaMijozlar;

            // Xatolik bermasligi uchun havfsiz ulash
            var dgPopup = this.FindName("dgPopupMijozlar") as DataGrid;
            if (dgPopup != null) dgPopup.ItemsSource = BazaMijozlar;
            popupMijozlarView = CollectionViewSource.GetDefaultView(BazaMijozlar);
            popupKassaKimgaView = CollectionViewSource.GetDefaultView(KassaKimgaOptions);
            kassaKirimView = CollectionViewSource.GetDefaultView(KassaKirimlar);
            omborView = CollectionViewSource.GetDefaultView(IshlabChiqarishlar);

            if (dgSozlamaPudratchilar != null) dgSozlamaPudratchilar.ItemsSource = BazaPudratchilar;
            if (dgSozlamaHodimlar != null) dgSozlamaHodimlar.ItemsSource = BazaHodimlar;
            if (dgSozlamaHomAshyo != null) dgSozlamaHomAshyo.ItemsSource = BazaHomAshyo;
            if (dgSozlamaTayyorMahsulot != null) dgSozlamaTayyorMahsulot.ItemsSource = BazaTayyorMahsulot;
            if (dgSozlamaOlchov != null) dgSozlamaOlchov.ItemsSource = BazaOlchovlar;
            if (dgXarid != null) dgXarid.ItemsSource = Xaridlar;
            if (dgAktMijoz != null) dgAktMijoz.ItemsSource = AktMijozItems;
            if (dgAktPudratchi != null) dgAktPudratchi.ItemsSource = AktPudratchiItems;
            if (cmbAktMijoz != null) cmbAktMijoz.ItemsSource = BazaMijozlar;
            if (cmbAktPudratchi != null) cmbAktPudratchi.ItemsSource = BazaPudratchilar;

            if (txtMilliyBankKurs != null) txtMilliyBankKurs.Text = milliyBankUsdKursi.ToString("N2");
            if (txtSozlamaBankKursi != null) txtSozlamaBankKursi.Text = milliyBankUsdKursi.ToString("N2");
            if (txtXaridKurs != null) txtXaridKurs.Text = milliyBankUsdKursi.ToString("N2");
            if (dpOmborKunlikSana != null) dpOmborKunlikSana.SelectedDate = DateTime.Today;
            if (dpSanaDan != null) dpSanaDan.SelectedDate = DateTime.Today;
            if (dpSanaGacha != null) dpSanaGacha.SelectedDate = DateTime.Today;
            if (dpJujaSana != null) dpJujaSana.SelectedDate = DateTime.Today;
            if (dpKassaSana != null) dpKassaSana.SelectedDate = DateTime.Today;
            if (txtKassaKunBoshi != null) txtKassaKunBoshi.Text = "0";
            SeedKassaRashodTurlari();
            ApplySavedLanguageSelection();
            AttachGlobalDateInputBehavior();

            SeedDemoData();
            RefreshKassaKimgaOptionsByType();
            SyncKassaKirimlarFromCheklar();
            ApplyOmborDateFilter();
            RefreshDashboard();
            RefreshKassaHisobi();
            ApplySelectedLanguage();
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplySelectedLanguage();
        }

        private void ApplySelectedLanguage()
        {
            string lang = (cmbLanguage?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "uz";
            if (lang != "ru") lang = "uz";
            currentLanguage = lang;
            this.Title = TranslateText("Parrandachilik Buxgalteriyasi", lang);
            TranslateElementTree(this, lang);
            SaveLanguage(lang);
        }

        private void ApplySavedLanguageSelection()
        {
            string lang = "uz";
            try
            {
                if (File.Exists(languageStatePath))
                {
                    string saved = File.ReadAllText(languageStatePath).Trim().ToLowerInvariant();
                    if (saved == "ru" || saved == "uz") lang = saved;
                }
            }
            catch { }

            if (cmbLanguage == null) return;
            foreach (ComboBoxItem item in cmbLanguage.Items)
            {
                string tag = item.Tag?.ToString();
                if (tag == lang)
                {
                    cmbLanguage.SelectedItem = item;
                    return;
                }
            }
            cmbLanguage.SelectedIndex = 0;
        }

        private void SaveLanguage(string lang)
        {
            try
            {
                string dir = Path.GetDirectoryName(languageStatePath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(languageStatePath, lang);
            }
            catch { }
        }

        private string L(string uz, string ru) => currentLanguage == "ru" ? ru : uz;

        private void ShowMsg(string uz, string ru)
        {
            MessageBox.Show(L(uz, ru));
        }

        private MessageBoxResult AskYesNo(string uzMessage, string ruMessage, string uzTitle = "Tasdiqlash", string ruTitle = "Подтверждение")
        {
            return MessageBox.Show(
                L(uzMessage, ruMessage),
                L(uzTitle, ruTitle),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
        }

        private void TranslateElementTree(DependencyObject root, string lang)
        {
            if (root is ContentControl contentControl && contentControl.Content is string contentText)
            {
                contentControl.Content = TranslateText(contentText, lang);
            }
            if (root is HeaderedContentControl headeredContentControl && headeredContentControl.Header is string headerText)
            {
                headeredContentControl.Header = TranslateText(headerText, lang);
            }
            if (root is TextBlock textBlock)
            {
                textBlock.Text = TranslateText(textBlock.Text, lang);
            }
            if (root is FrameworkElement frameworkElement && frameworkElement.ToolTip is string tooltip)
            {
                frameworkElement.ToolTip = TranslateText(tooltip, lang);
            }

            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                TranslateElementTree(VisualTreeHelper.GetChild(root, i), lang);
            }
        }

        private string TranslateText(string source, string lang)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            foreach (var entry in uiTexts)
            {
                if (source == entry.Base || source == entry.Uz || source == entry.Ru)
                {
                    return lang == "ru" ? entry.Ru : entry.Uz;
                }
            }
            return source;
        }

        private void SeedKassaRashodTurlari()
        {
            if (KassaRashodTurlari.Count > 0) return;
            KassaRashodTurlari.Add("Оплата поставщику");
            KassaRashodTurlari.Add("Расход подотчетному лицу");
            KassaRashodTurlari.Add("Прочее списание");
            KassaRashodTurlari.Add("Выплата заработной платы сотруднику");
            if (cmbKassaRashodTuri != null) cmbKassaRashodTuri.SelectedIndex = 0;
        }

        private void SyncKassaKirimlarFromCheklar()
        {
            var holatlar = KassaKirimlar.ToDictionary(k => k.ChekID, k => k.Holat);
            KassaKirimlar.Clear();
            foreach (var chek in Cheklar
                .Where(c => c.Naqd > 0)
                .OrderBy(c => c.ID))
            {
                KassaKirimlar.Add(new KassaKirimModel
                {
                    ChekID = chek.ID,
                    Sana = chek.Sana,
                    Mijoz = chek.Mijoz,
                    Naqd = chek.Naqd,
                    Holat = holatlar.TryGetValue(chek.ID, out string holat) ? holat : "Kutilmoqda"
                });
            }
            ApplyKassaKirimDateFilter();
            dgKassaKirim?.Items.Refresh();
        }

        private void ApplyKassaKirimDateFilter()
        {
            if (kassaKirimView == null) return;
            DateTime targetDate = GetOperationalDate();
            kassaKirimView.Filter = obj =>
            {
                var item = obj as KassaKirimModel;
                if (item == null) return false;
                return ParseDateOrToday(item.Sana) == targetDate;
            };
            kassaKirimView.Refresh();
        }

        private DateTime GetOperationalDate()
        {
            if (dpKassaSana?.SelectedDate is DateTime kassaSana) return kassaSana.Date;
            if (dpChekSanaFiltr?.SelectedDate is DateTime sanaPicker) return sanaPicker.Date;
            if (DateTime.TryParse(txtSanaFiltr?.Text, out DateTime sanaText)) return sanaText.Date;
            return DateTime.Today;
        }

        private void DpKassaSana_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyKassaKirimDateFilter();
            RefreshKassaHisobi();
        }

        private void SeedDemoData()
        {
            if (IshlabChiqarishlar.Count > 0) return;
            IshlabChiqarishlar.Add(new TuxumModel
            {
                ID = 1,
                Sana = DateTime.Now.ToString("dd.MM.yyyy"),
                SexNomi = "1-sex",
                YarusNomi = "Yarus-1",
                HodimFISH = "Dilfuza",
                ParrandaSoni = 2400,
                NobudSoni = 3,
                YemKg = 310,
                W1_6 = 250,
                W1_7 = 340,
                W1_8 = 300,
                W1_9 = 260,
                W2_0 = 200,
                W2_5 = 90,
                Siniq = 15,
                Paket = 20,
                Tasdiqlash = "Kutilmoqda"
            });
            IshlabChiqarishlar.Add(new TuxumModel
            {
                ID = 2,
                Sana = DateTime.Now.ToString("dd.MM.yyyy"),
                SexNomi = "2-sex",
                YarusNomi = "Yarus-2",
                HodimFISH = "Gulbahor",
                ParrandaSoni = 2600,
                NobudSoni = 4,
                YemKg = 355,
                W1_6 = 270,
                W1_7 = 360,
                W1_8 = 320,
                W1_9 = 280,
                W2_0 = 210,
                W2_5 = 100,
                Siniq = 18,
                Paket = 25,
                Tasdiqlash = "Kutilmoqda"
            });
            Sexlar.Add(new SexModel { Nomi = "1-sex" });
            Sexlar.Add(new SexModel { Nomi = "2-sex" });
            Yaruslar.Add(new YarusModel { SexNomi = "1-sex", Nomi = "Yarus-1" });
            Yaruslar.Add(new YarusModel { SexNomi = "2-sex", Nomi = "Yarus-2" });
            if (cmbIshlabSex != null && Sexlar.Count > 0) cmbIshlabSex.SelectedIndex = 0;
            RefreshIshlabYaruslar();
            if (cmbIshlabYarus != null && IshlabYaruslar.Count > 0) cmbIshlabYarus.SelectedIndex = 0;
            if (cmbJujaSex != null && Sexlar.Count > 0) cmbJujaSex.SelectedIndex = 0;
            RefreshJujaYaruslar();
            if (cmbJujaYarus != null && JujaYaruslar.Count > 0) cmbJujaYarus.SelectedIndex = 0;
        }

        private void RefreshIshlabYaruslar()
        {
            string sexNomi = (cmbIshlabSex?.SelectedItem as SexModel)?.Nomi;
            IshlabYaruslar.Clear();
            foreach (var item in Yaruslar.Where(x => string.Equals(x.SexNomi, sexNomi, StringComparison.OrdinalIgnoreCase)))
            {
                IshlabYaruslar.Add(item);
            }
            if (cmbIshlabYarus != null && IshlabYaruslar.Count > 0) cmbIshlabYarus.SelectedIndex = 0;
        }

        private void RefreshJujaYaruslar()
        {
            string sexNomi = (cmbJujaSex?.SelectedItem as SexModel)?.Nomi;
            JujaYaruslar.Clear();
            foreach (var item in Yaruslar.Where(x => string.Equals(x.SexNomi, sexNomi, StringComparison.OrdinalIgnoreCase)))
            {
                JujaYaruslar.Add(item);
            }
            if (cmbJujaYarus != null && JujaYaruslar.Count > 0) cmbJujaYarus.SelectedIndex = 0;
        }

        // ================= 1. HISOBLASH MANTIG'I =================
        private void Calculate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ChekMahsulotlar == null) return;
            double jamiSumma = 0, jamiSoni = 0;
            foreach (var item in ChekMahsulotlar) { jamiSumma += item.Summa; jamiSoni += item.Soni; }

            if (txtChekSumma != null) txtChekSumma.Text = jamiSumma.ToString("N2");
            if (txtJamiSoni != null) txtJamiSoni.Text = jamiSoni.ToString("N0");

            double kochirilgan = ParseDouble(txtKochirilgan?.Text);
            double karta = ParseDouble(txtKarta?.Text);
            double naqdInput = ParseDouble(txtNaqdInput?.Text);
            double eskiHaq = ParseDouble(txtHaq?.Text);

            double qarzHaqHolati = (eskiHaq + jamiSumma) - (naqdInput + karta + kochirilgan);
            if (txtBuHaq != null) txtBuHaq.Text = qarzHaqHolati.ToString("N2");
        }

        private void TxtXarajat_TextChanged(object sender, TextChangedEventArgs e)
        {
            double bosh = ParseDouble(txtBoshQoldiq?.Text);
            double kirimNaqd = ParseDouble(txtKirimNaqd?.Text);
            double kirimKarta = ParseDouble(txtKirimKarta?.Text);
            double kirimKoch = ParseDouble(txtKirimKochirilgan?.Text);
            double xarajatNaqd = ParseDouble(txtXarajatNaqd?.Text);

            double jamiKirim = kirimNaqd + kirimKarta + kirimKoch;
            if (txtKirimJami != null) txtKirimJami.Text = jamiKirim.ToString("N2");

            var txtXarJami = this.FindName("txtXarajatJami") as TextBlock;
            if (txtXarJami != null) txtXarJami.Text = xarajatNaqd.ToString("N2");

            double yakuniy = bosh + jamiKirim - xarajatNaqd;
            if (txtYakuniyQoldiq != null) txtYakuniyQoldiq.Text = yakuniy.ToString("N2");
        }

        private void DgChek_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                UIElement focusedElement = Keyboard.FocusedElement as UIElement;
                focusedElement?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void DgChek_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => Calculate_TextChanged(null, null)), System.Windows.Threading.DispatcherPriority.Background);
        }
        private double ParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            text = text.Replace(" ", "").Replace(",", ".");
            double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double res);
            return res;
        }
        private DateTime ParseDateOrToday(string text)
        {
            if (TryNormalizeDateText(text, out string normalized, out DateTime dt)) return dt.Date;
            if (DateTime.TryParse(normalized ?? text, out dt)) return dt.Date;
            return DateTime.Today;
        }

        // ================= 2. SANA MANTIG'I =================
        private void DpChekSanaFiltr_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpChekSanaFiltr != null && dpChekSanaFiltr.SelectedDate.HasValue)
            {
                string t = dpChekSanaFiltr.SelectedDate.Value.ToString("dd.MM.yyyy");
                if (txtSanaFiltr != null) txtSanaFiltr.Text = t;
                if (txtMaxsusSana != null) txtMaxsusSana.Text = t;
            }
            RefreshSotuvOmborKunlik();
            ApplyKassaKirimDateFilter();
            SyncKassaKirimlarFromCheklar();
            RefreshKassaHisobi();
        }

        private void TxtSanaFiltr_LostFocus(object sender, RoutedEventArgs e)
        {
            FormatStringDate(txtSanaFiltr);
            if (txtMaxsusSana != null) txtMaxsusSana.Text = txtSanaFiltr.Text;
            if (DateTime.TryParse(txtSanaFiltr.Text, out DateTime dt) && dpChekSanaFiltr != null)
                dpChekSanaFiltr.SelectedDate = dt;
            RefreshSotuvOmborKunlik();
            ApplyKassaKirimDateFilter();
            SyncKassaKirimlarFromCheklar();
            RefreshKassaHisobi();
        }

        private void TxtMaxsusSana_LostFocus(object sender, RoutedEventArgs e)
        {
            FormatStringDate(txtMaxsusSana);
            if (txtSanaFiltr != null) txtSanaFiltr.Text = txtMaxsusSana.Text;
            if (DateTime.TryParse(txtMaxsusSana.Text, out DateTime dt) && dpChekSanaFiltr != null)
                dpChekSanaFiltr.SelectedDate = dt;
            RefreshSotuvOmborKunlik();
            ApplyKassaKirimDateFilter();
            SyncKassaKirimlarFromCheklar();
            RefreshKassaHisobi();
        }

        private void FormatStringDate(TextBox tb)
        {
            if (tb == null) return;
            if (TryNormalizeDateText(tb.Text, out string formatted, out _))
                tb.Text = formatted;
        }

        private void BtnSanaUp_Click(object sender, RoutedEventArgs e) { SanaOshirishKamaytirish(txtMaxsusSana, 1); }
        private void BtnSanaDown_Click(object sender, RoutedEventArgs e) { SanaOshirishKamaytirish(txtMaxsusSana, -1); }
        private void BtnSanaFiltrUp_Click(object sender, RoutedEventArgs e) { SanaOshirishKamaytirish(txtSanaFiltr, 1); }
        private void BtnSanaFiltrDown_Click(object sender, RoutedEventArgs e) { SanaOshirishKamaytirish(txtSanaFiltr, -1); }

        private void SanaOshirishKamaytirish(TextBox tb, int kun)
        {
            if (tb == null) return;
            if (DateTime.TryParse(tb.Text, out DateTime dt))
            {
                string yangiSana = dt.AddDays(kun).ToString("dd.MM.yyyy");
                if (txtMaxsusSana != null) txtMaxsusSana.Text = yangiSana;
                if (txtSanaFiltr != null) txtSanaFiltr.Text = yangiSana;
                if (dpChekSanaFiltr != null) dpChekSanaFiltr.SelectedDate = dt.AddDays(kun);
            }
        }

        private bool TryNormalizeDateText(string input, out string formatted, out DateTime parsedDate)
        {
            formatted = input;
            parsedDate = DateTime.Today;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string clean = new string(input.Where(char.IsDigit).ToArray());
            if (clean.Length == 8)
            {
                string candidate = clean.Substring(0, 2) + "." + clean.Substring(2, 2) + "." + clean.Substring(4, 4);
                if (DateTime.TryParse(candidate, out parsedDate))
                {
                    formatted = parsedDate.ToString("dd.MM.yyyy");
                    return true;
                }
            }

            if (DateTime.TryParse(input, out parsedDate))
            {
                formatted = parsedDate.ToString("dd.MM.yyyy");
                return true;
            }
            return false;
        }

        private void AttachGlobalDateInputBehavior()
        {
            AttachDateBehaviorRecursive(this);
        }

        private void AttachDateBehaviorRecursive(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var tb = child as TextBox;
                if (tb != null && !tb.IsReadOnly && !string.IsNullOrWhiteSpace(tb.Name) &&
                    tb.Name.IndexOf("Sana", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    tb.MaxLength = 10;
                    tb.PreviewTextInput -= DateTextBox_PreviewTextInput;
                    tb.PreviewTextInput += DateTextBox_PreviewTextInput;
                    tb.LostFocus -= DateTextBox_LostFocus;
                    tb.LostFocus += DateTextBox_LostFocus;
                    if (string.IsNullOrWhiteSpace(tb.ToolTip?.ToString())) tb.ToolTip = "dd.MM.yyyy";
                }

                var dp = child as DatePicker;
                if (dp != null)
                {
                    dp.Loaded -= DatePicker_Loaded;
                    dp.Loaded += DatePicker_Loaded;
                    dp.GotFocus -= DatePicker_GotFocus;
                    dp.GotFocus += DatePicker_GotFocus;
                    dp.LostFocus -= DatePicker_LostFocus;
                    dp.LostFocus += DatePicker_LostFocus;
                }

                AttachDateBehaviorRecursive(child);
            }
        }

        private void DateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text)) return;
            char ch = e.Text[0];
            e.Handled = !(char.IsDigit(ch) || ch == '.' || ch == '/' || ch == '-');
        }

        private void DateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb) FormatStringDate(tb);
        }

        private void DatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;
            if (TryNormalizeDateText(dp.Text, out string formatted, out DateTime date))
            {
                dp.SelectedDate = date;
                dp.Text = formatted;
                return;
            }
            if (!dp.SelectedDate.HasValue) dp.Text = "__.__.____";
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;
            ApplyDatePickerMask(dp);
            var innerTb = FindDatePickerTextBox(dp);
            if (innerTb != null)
            {
                innerTb.GotFocus -= DatePickerTextBox_GotFocus;
                innerTb.GotFocus += DatePickerTextBox_GotFocus;
                innerTb.LostFocus -= DatePickerTextBox_LostFocus;
                innerTb.LostFocus += DatePickerTextBox_LostFocus;
            }
        }

        private void DatePicker_GotFocus(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;
            if (dp == null) return;
            if (dp.Text == "__.__.____") dp.Text = string.Empty;
        }

        private void DatePickerTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as DatePickerTextBox;
            if (tb != null && tb.Text == "__.__.____") tb.Text = string.Empty;
        }

        private void DatePickerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as DatePickerTextBox;
            if (tb == null) return;
            if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "__.__.____";
        }

        private void ApplyDatePickerMask(DatePicker dp)
        {
            if (dp == null) return;
            if (dp.SelectedDate.HasValue)
            {
                dp.Text = dp.SelectedDate.Value.ToString("dd.MM.yyyy");
                return;
            }
            if (string.IsNullOrWhiteSpace(dp.Text) || dp.Text == "__/__/____")
            {
                dp.Text = "__.__.____";
            }
        }

        private DatePickerTextBox FindDatePickerTextBox(DatePicker dp)
        {
            if (dp.Template == null) return null;
            return dp.Template.FindName("PART_TextBox", dp) as DatePickerTextBox;
        }

        // ================= 3. TUGMALAR =================
        private void BtnYangiChek_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(string.IsNullOrWhiteSpace(txtChekNum.Text) ? "0" : txtChekNum.Text);
            txtChekNum.Text = (id + 1).ToString();
            BtnTozalash_Click(null, null);
        }

        private void BtnTozalash_Click(object sender, RoutedEventArgs e)
        {
            if (txtNaqdInput != null) txtNaqdInput.Clear();
            if (txtKarta != null) txtKarta.Clear();
            if (txtKochirilgan != null) txtKochirilgan.Clear();
            var txtMijoz = this.FindName("txtTanlanganMijoz") as TextBox;
            if (txtMijoz != null) txtMijoz.Clear();
            if (txtHaq != null) txtHaq.Clear();
            if (txtBuHaq != null) txtBuHaq.Clear();
            foreach (var i in ChekMahsulotlar) { i.Narx = 0; i.Soni = 0; i.Bonus = 0; }
            Calculate_TextChanged(null, null);
        }

        private void BtnDeleteChek_Click(object sender, RoutedEventArgs e) => ShowMsg("Tanlangan chek o'chirildi!", "Выбранный чек удален!");
        private void BtnSaveChek_Click(object sender, RoutedEventArgs e)
        {
            if (IshlabChiqarishlar.Any(x => x.Tasdiqlash == "Qaytarildi"))
            {
                ShowMsg(GetSotuvBlockReason(), "Продажа заблокирована.");
                return;
            }

            double summa = ParseDouble(txtChekSumma?.Text);
            if (summa <= 0)
            {
                ShowMsg("Chek bo'sh. Avval mahsulot kiriting.", "Чек пуст. Сначала добавьте товар.");
                return;
            }

            string mijoz = txtTanlanganMijoz?.Text;
            if (string.IsNullOrWhiteSpace(mijoz)) mijoz = "Noma'lum mijoz";
            DateTime chekSana = ParseDateOrToday(string.IsNullOrWhiteSpace(txtMaxsusSana?.Text) ? DateTime.Today.ToString("dd.MM.yyyy") : txtMaxsusSana.Text);

            Cheklar.Add(new ChekModel
            {
                ID = Cheklar.Count + 1,
                Mijoz = mijoz,
                Summa = summa,
                Sana = chekSana.ToString("dd.MM.yyyy"),
                Naqd = ParseDouble(txtNaqdInput?.Text),
                Karta = ParseDouble(txtKarta?.Text),
                Kochirilgan = ParseDouble(txtKochirilgan?.Text),
                EskiHaq = ParseDouble(txtHaq?.Text),
                BuHaq = ParseDouble(txtBuHaq?.Text),
                HolatYashil = false,
                SotuvHolati = "Sariq",
                OmborHolati = "Kutilmoqda",
                Items = ChekMahsulotlar
                    .Where(x => x.Soni > 0)
                    .Select(x => new ChekItemSnapshotModel
                    {
                        Mahsulot = x.Mahsulot,
                        Narx = x.Narx,
                        Soni = x.Soni,
                        Summa = x.Summa
                    })
                    .ToList()
            });

            foreach (var row in ChekMahsulotlar.Where(x => x.Soni > 0))
            {
                var ombor = OmborHolat.FirstOrDefault(x => x.Mahsulot == row.Mahsulot);
                if (ombor != null) ombor.Sotildi += row.Soni;
                SotuvHarakatlar.Add(new OmborHarakatModel { Sana = chekSana, Mahsulot = row.Mahsulot, Miqdor = row.Soni });
            }
            SyncKassaKirimlarFromCheklar();

            BtnTozalash_Click(null, null);
            if (txtChekNum != null) txtChekNum.Text = (Cheklar.Count + 1).ToString();
            if (dgSotuvOmborHolati != null) dgSotuvOmborHolati.Items.Refresh();
            if (dgCheklar != null) dgCheklar.Items.Refresh();
            RefreshDashboard();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (Cheklar.Count == 0)
            {
                ShowMsg("Print uchun kamida bitta saqlangan chek bo'lishi kerak.", "Для печати нужен минимум один сохраненный чек.");
                return;
            }

            var chek = dgCheklar?.SelectedItem as ChekModel;
            if (chek == null)
            {
                ShowMsg("Print qilish uchun chap ro'yxatdan bitta chek tanlang.", "Для печати выберите один чек из левого списка.");
                return;
            }

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;

            FlowDocument doc = BuildChekPrintDocument(chek);
            doc.PageWidth = pd.PrintableAreaWidth;
            doc.PageHeight = pd.PrintableAreaHeight;
            doc.PagePadding = new Thickness(25);
            doc.ColumnWidth = pd.PrintableAreaWidth;

            IDocumentPaginatorSource idp = doc;
            pd.PrintDocument(idp.DocumentPaginator, $"Chek_{chek.ID}_{chek.Mijoz}");
            ShowMsg("Chek printerga yuborildi.", "Чек отправлен на принтер.");
        }

        private FlowDocument BuildChekPrintDocument(ChekModel chek)
        {
            var doc = new FlowDocument();
            doc.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            doc.FontSize = 12;

            doc.Blocks.Add(new Paragraph(new Run("SOTUV CHEKI (1 KLIENT)"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Chek № {chek.ID}    Sana: {chek.Sana}    Mijoz: {chek.Mijoz}")));
            doc.Blocks.Add(new Paragraph(new Run($"Haq oqimi: ({chek.EskiHaq:N2} + {chek.Summa:N2}) - {chek.Karta:N2} - {chek.Kochirilgan:N2} - {chek.Naqd:N2} = {chek.BuHaq:N2}"))
            {
                FontWeight = FontWeights.SemiBold
            });

            doc.Blocks.Add(new Paragraph(new Run("SOTUV BO'LIMI (narx va summa bilan)"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 4)
            });
            Table sotuvTable = new Table();
            sotuvTable.Columns.Add(new TableColumn { Width = new GridLength(220) });
            sotuvTable.Columns.Add(new TableColumn { Width = new GridLength(90) });
            sotuvTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
            sotuvTable.Columns.Add(new TableColumn { Width = new GridLength(110) });
            TableRowGroup sotuvGroup = new TableRowGroup();
            sotuvGroup.Rows.Add(MakeRow(true, "Mahsulot", "Narx", "Soni", "Summa"));
            foreach (var i in chek.Items) sotuvGroup.Rows.Add(MakeRow(false, i.Mahsulot, i.Narx.ToString("N2"), i.Soni.ToString("N2"), i.Summa.ToString("N2")));
            sotuvGroup.Rows.Add(MakeRow(true, "Jami", "", "", chek.Summa.ToString("N2")));
            sotuvTable.RowGroups.Add(sotuvGroup);
            doc.Blocks.Add(sotuvTable);

            doc.Blocks.Add(new Paragraph(new Run("KASSA BO'LIMI (faqat to'lovlar)"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 4)
            });
            doc.Blocks.Add(new Paragraph(new Run($"Karta: {chek.Karta:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Ko'chirilgan: {chek.Kochirilgan:N2}")));
            doc.Blocks.Add(new Paragraph(new Run($"Naqd: {chek.Naqd:N2}")));

            doc.Blocks.Add(new Paragraph(new Run("OMBOR BO'LIMI (faqat son)"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 4)
            });
            Table omborTable = new Table();
            omborTable.Columns.Add(new TableColumn { Width = new GridLength(240) });
            omborTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
            TableRowGroup omborGroup = new TableRowGroup();
            omborGroup.Rows.Add(MakeRow(true, "Mahsulot", "Soni"));
            foreach (var i in chek.Items) omborGroup.Rows.Add(MakeRow(false, i.Mahsulot, i.Soni.ToString("N2")));
            omborTable.RowGroups.Add(omborGroup);
            doc.Blocks.Add(omborTable);

            return doc;
        }

        private TableRow MakeRow(bool bold, params string[] cells)
        {
            TableRow r = new TableRow();
            foreach (var c in cells)
            {
                var p = new Paragraph(new Run(c)) { Margin = new Thickness(4, 2, 4, 2) };
                if (bold) p.FontWeight = FontWeights.SemiBold;
                r.Cells.Add(new TableCell(p) { BorderBrush = System.Windows.Media.Brushes.LightGray, BorderThickness = new Thickness(0.5) });
            }
            return r;
        }

        // ================= POPUP MIJOZ =================
        private void BtnMijozMenu_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("popMijozMenu") as System.Windows.Controls.Primitives.Popup;
            if (popup != null) popup.IsOpen = true;
        }
        private void BtnOpenYangiMijozTab_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("popMijozMenu") as System.Windows.Controls.Primitives.Popup;
            if (popup != null) popup.IsOpen = true;
            if (tabPopMijozMenu != null) tabPopMijozMenu.SelectedIndex = 1;
        }

        private void TxtPopupMijozFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (popupMijozlarView == null) return;
            string kalit = txtPopupMijozFilter?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(kalit))
            {
                popupMijozlarView.Filter = null;
            }
            else
            {
                popupMijozlarView.Filter = obj =>
                {
                    if (obj is MijozModel m)
                    {
                        return (m.Nomi ?? string.Empty).IndexOf(kalit, StringComparison.OrdinalIgnoreCase) >= 0
                            || (m.Tel ?? string.Empty).IndexOf(kalit, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    return false;
                };
            }
            popupMijozlarView.Refresh();
        }

        private void DgPopupMijozlar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dgPopup = sender as DataGrid;
            if (dgPopup != null && dgPopup.SelectedItem is MijozModel tanlangan)
            {
                var txtMijoz = this.FindName("txtTanlanganMijoz") as TextBox;
                if (txtMijoz != null) txtMijoz.Text = tanlangan.Nomi;
                var popup = this.FindName("popMijozMenu") as System.Windows.Controls.Primitives.Popup;
                if (popup != null) popup.IsOpen = false;
            }
        }

        private void BtnPopMijozSaqlash_Click(object sender, RoutedEventArgs e)
        {
            var ism = this.FindName("txtPopYangiMijoz") as TextBox;
            var tel = this.FindName("txtPopYangiTel") as TextBox;
            var manzil = this.FindName("txtPopYangiManzil") as TextBox;

            if (ism != null && !string.IsNullOrWhiteSpace(ism.Text))
            {
                var yangi = new MijozModel { Nomi = ism.Text, Tel = tel?.Text, Manzil = manzil?.Text };
                BazaMijozlar.Add(yangi);

                var txtMijoz = this.FindName("txtTanlanganMijoz") as TextBox;
                if (txtMijoz != null) txtMijoz.Text = yangi.Nomi;

                var popup = this.FindName("popMijozMenu") as System.Windows.Controls.Primitives.Popup;
                if (popup != null) popup.IsOpen = false;

                ism.Clear(); tel?.Clear(); manzil?.Clear();
                if (popupMijozlarView != null) popupMijozlarView.Refresh();
            }
        }

        private void BtnPopKatalogOlish_Click(object sender, RoutedEventArgs e)
        {
            var popup = this.FindName("popMijozMenu") as System.Windows.Controls.Primitives.Popup;
            if (popup != null) popup.IsOpen = false;
            mainTabControl.SelectedIndex = 12; // Sozlamalarga o'tish
        }

        // ================= BOSHQA BAZA TUGMALARI =================
        private void BtnAddMijoz_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSozlamaMijozNomi.Text)) return;
            BazaMijozlar.Add(new MijozModel
            {
                Nomi = txtSozlamaMijozNomi.Text,
                Tel = txtSozlamaMijozTel.Text,
                Manzil = txtSozlamaMijozManzil.Text,
                ShartnomaSana = dpMijozSana.SelectedDate?.ToString("dd.MM.yyyy"),
                ShartnomaRaqami = txtMijozShartnomaNo.Text
            });
            txtSozlamaMijozNomi.Clear(); txtSozlamaMijozTel.Clear(); txtSozlamaMijozManzil.Clear(); txtMijozShartnomaNo.Clear();
        }
        private void BtnAddPudratchi_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPudratchiNomi.Text)) return;
            BazaPudratchilar.Add(new PudratchiModel { Nomi = txtPudratchiNomi.Text, Tel = txtPudratchiTel.Text, Manzil = txtPudratchiManzil.Text, ShartnomaSana = dpPudratchiSana.SelectedDate?.ToString("dd.MM.yyyy"), ShartnomaRaqami = txtPudratchiShartnomaNo.Text });
            txtPudratchiNomi.Clear(); txtPudratchiTel.Clear(); txtPudratchiManzil.Clear(); txtPudratchiShartnomaNo.Clear();
        }
        private void BtnAddHodim_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHodimFISH.Text)) return;
            BazaHodimlar.Add(new HodimModel { FISH = txtHodimFISH.Text, Tel = txtHodimTel.Text, ShartnomaSana = dpHodimSana.SelectedDate?.ToString("dd.MM.yyyy") });
            txtHodimFISH.Clear(); txtHodimTel.Clear();
        }
        private void BtnAddHomAshyo_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(txtHomAshyoNomi.Text)) { BazaHomAshyo.Add(new OddiyKatalogModel { Nomi = txtHomAshyoNomi.Text }); txtHomAshyoNomi.Clear(); } }
        private void BtnAddTayyorMahsulot_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(txtTayyorMahsulotNomi.Text)) { BazaTayyorMahsulot.Add(new OddiyKatalogModel { Nomi = txtTayyorMahsulotNomi.Text }); txtTayyorMahsulotNomi.Clear(); } }
        private void BtnAddOlchov_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrWhiteSpace(txtOlchovNomi.Text)) { BazaOlchovlar.Add(new OddiyKatalogModel { Nomi = txtOlchovNomi.Text }); txtOlchovNomi.Clear(); } }
        private void BtnSaveBankKursi_Click(object sender, RoutedEventArgs e)
        {
            double yangiKurs = ParseDouble(txtSozlamaBankKursi?.Text);
            if (yangiKurs <= 0)
            {
                ShowMsg("Kurs 0 dan katta bo'lishi kerak.", "Курс должен быть больше 0.");
                return;
            }
            milliyBankUsdKursi = yangiKurs;
            if (txtMilliyBankKurs != null) txtMilliyBankKurs.Text = milliyBankUsdKursi.ToString("N2");
            if (txtXaridKurs != null) txtXaridKurs.Text = milliyBankUsdKursi.ToString("N2");
            ShowMsg("Milliy bank kursi saqlandi.", "Курс Нацбанка сохранен.");
        }

        private void BtnApplyKurs_Click(object sender, RoutedEventArgs e)
        {
            double yangiKurs = ParseDouble(txtMilliyBankKurs?.Text);
            if (yangiKurs <= 0)
            {
                ShowMsg("Kurs 0 dan katta bo'lishi kerak.", "Курс должен быть больше 0.");
                return;
            }
            milliyBankUsdKursi = yangiKurs;
            if (txtSozlamaBankKursi != null) txtSozlamaBankKursi.Text = milliyBankUsdKursi.ToString("N2");
            if (txtXaridKurs != null) txtXaridKurs.Text = milliyBankUsdKursi.ToString("N2");
        }

        private void BtnAddXarid_Click(object sender, RoutedEventArgs e)
        {
            string nomi = txtXaridNomi?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(nomi))
            {
                ShowMsg("Yem nomini kiriting.", "Введите название корма.");
                return;
            }

            double miqdor = ParseDouble(txtXaridMiqdori?.Text);
            double narx = ParseDouble(txtXaridNarx?.Text);
            double kurs = ParseDouble(txtXaridKurs?.Text);
            if (miqdor <= 0 || narx <= 0)
            {
                ShowMsg("Miqdor va narx 0 dan katta bo'lishi kerak.", "Количество и цена должны быть больше 0.");
                return;
            }

            string valyuta = ((cmbXaridValyuta?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "UZS").ToUpperInvariant();
            if (valyuta == "USD" && kurs <= 0)
            {
                ShowMsg("USD uchun kursni to'g'ri kiriting.", "Введите корректный курс для USD.");
                return;
            }
            if (kurs <= 0) kurs = milliyBankUsdKursi;

            double jamiUzs = valyuta == "USD" ? narx * miqdor * kurs : narx * miqdor;

            Xaridlar.Add(new XaridModel
            {
                Sana = DateTime.Now.ToString("dd.MM.yyyy"),
                Nomi = nomi,
                Miqdori = miqdor,
                Valyuta = valyuta,
                Narx = narx,
                Kurs = kurs,
                JamiUzs = jamiUzs,
                Izoh = txtXaridIzoh?.Text
            });

            txtXaridNomi?.Clear();
            txtXaridMiqdori?.Clear();
            txtXaridNarx?.Clear();
            txtXaridIzoh?.Clear();
            if (txtXaridKurs != null) txtXaridKurs.Text = milliyBankUsdKursi.ToString("N2");
            RefreshDashboard();
        }

        private void BtnCalcAktMijoz_Click(object sender, RoutedEventArgs e)
        {
            AktMijozItems.Clear();
            double saldo = 0;
            string mijoz = (cmbAktMijoz?.SelectedItem as MijozModel)?.Nomi;
            DateTime? dan = dpAktMijozDan?.SelectedDate;
            DateTime? gacha = dpAktMijozGacha?.SelectedDate;
            foreach (var item in Cheklar)
            {
                if (!string.IsNullOrWhiteSpace(mijoz) && item.Mijoz != mijoz) continue;
                if (DateTime.TryParse(item.Sana, out DateTime sana))
                {
                    if (dan.HasValue && sana.Date < dan.Value.Date) continue;
                    if (gacha.HasValue && sana.Date > gacha.Value.Date) continue;
                }

                saldo += item.Summa;
                AktMijozItems.Add(new AktSverkaModel
                {
                    Sana = item.Sana,
                    Hujjat = "Sotuv cheki",
                    Kirim = item.Summa,
                    Chiqim = 0,
                    Saldo = saldo,
                    Izoh = item.Mijoz
                });
            }
        }

        private void BtnCalcAktPudratchi_Click(object sender, RoutedEventArgs e)
        {
            AktPudratchiItems.Clear();
            double saldo = 0;
            DateTime? dan = dpAktPudrDan?.SelectedDate;
            DateTime? gacha = dpAktPudrGacha?.SelectedDate;
            foreach (var item in Xaridlar)
            {
                if (DateTime.TryParse(item.Sana, out DateTime sana))
                {
                    if (dan.HasValue && sana.Date < dan.Value.Date) continue;
                    if (gacha.HasValue && sana.Date > gacha.Value.Date) continue;
                }
                saldo += item.JamiUzs;
                AktPudratchiItems.Add(new AktSverkaModel
                {
                    Sana = item.Sana,
                    Hujjat = "Xarid",
                    Kirim = 0,
                    Chiqim = item.JamiUzs,
                    Saldo = saldo,
                    Izoh = item.Nomi
                });
            }
        }
        private void BtnPrintAktMijoz_Click(object sender, RoutedEventArgs e)
        {
            if (AktMijozItems.Count == 0)
            {
                ShowMsg("Avval akt sverka hisoblang.", "Сначала рассчитайте акт сверки.");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel CSV (*.csv)|*.csv", FileName = "AktSverka_Mijoz.csv" };
            if (sfd.ShowDialog() != true) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Sana;Hujjat;Kirim;Chiqim;Saldo;Izoh");
            foreach (var row in AktMijozItems)
                sb.AppendLine($"{row.Sana};{row.Hujjat};{row.Kirim:N2};{row.Chiqim:N2};{row.Saldo:N2};{row.Izoh}");
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            ShowMsg("Mijoz akt-sverkasi chiqarildi.", "Акт сверки клиента сформирован.");
        }
        private void BtnPrintAktPudratchi_Click(object sender, RoutedEventArgs e)
        {
            if (AktPudratchiItems.Count == 0)
            {
                ShowMsg("Avval akt sverka hisoblang.", "Сначала рассчитайте акт сверки.");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel CSV (*.csv)|*.csv", FileName = "AktSverka_Pudratchi.csv" };
            if (sfd.ShowDialog() != true) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Sana;Hujjat;Kirim;Chiqim;Saldo;Izoh");
            foreach (var row in AktPudratchiItems)
                sb.AppendLine($"{row.Sana};{row.Hujjat};{row.Kirim:N2};{row.Chiqim:N2};{row.Saldo:N2};{row.Izoh}");
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            ShowMsg("Pudratchi akt-sverkasi chiqarildi.", "Акт сверки поставщика сформирован.");
        }

        private void BtnRefreshDashboard_Click(object sender, RoutedEventArgs e) => RefreshDashboard();

        private void RefreshDashboard()
        {
            double jamiSotuv = Cheklar.Sum(x => x.Summa);
            double jamiXarid = Xaridlar.Sum(x => x.JamiUzs);
            double jamiSoni = IshlabChiqarishlar.Sum(x => x.JamiTuxum);
            double boshSoni = IshlabChiqarishlar.Sum(x => x.ParrandaSoni);
            double nobudSoni = IshlabChiqarishlar.Sum(x => x.NobudSoni);
            double jamiYemKg = IshlabChiqarishlar.Sum(x => x.YemKg);
            double qabulQilinganTuxum = IshlabChiqarishlar.Where(x => x.Tasdiqlash == "Qabul qilindi").Sum(x => x.JamiTuxum);

            double birBoshgaYem = boshSoni > 0 ? jamiYemKg / boshSoni : 0;
            double tuxumFoiz = boshSoni > 0 ? (qabulQilinganTuxum / boshSoni) * 100 : 0;
            double nobudFoiz = boshSoni > 0 ? (nobudSoni / boshSoni) * 100 : 0;
            double sofFoyda = jamiSotuv - jamiXarid;

            int sariq = IshlabChiqarishlar.Count(x => x.Tasdiqlash == "Kutilmoqda");
            int yashil = IshlabChiqarishlar.Count(x => x.Tasdiqlash == "Qabul qilindi");
            int qizil = IshlabChiqarishlar.Count(x => x.Tasdiqlash == "Qaytarildi");

            if (txtKpiYemSarfi != null) txtKpiYemSarfi.Text = $"{birBoshgaYem:N3} kg";
            if (txtKpiTuxumFoiz != null) txtKpiTuxumFoiz.Text = $"{tuxumFoiz:N2} %";
            if (txtKpiNobudFoiz != null) txtKpiNobudFoiz.Text = $"{nobudFoiz:N2} %";
            if (txtKpiSofFoyda != null) txtKpiSofFoyda.Text = $"{sofFoyda:N2} so'm";
            if (txtHisobotSofFoyda != null) txtHisobotSofFoyda.Text = $"{sofFoyda:N2} so'm";

            if (txtStatusSariq != null) txtStatusSariq.Text = sariq.ToString();
            if (txtStatusYashil != null) txtStatusYashil.Text = yashil.ToString();
            if (txtStatusQizil != null) txtStatusQizil.Text = qizil.ToString();

            if (elSariq != null) elSariq.Fill = sariq > 0 ? System.Windows.Media.Brushes.Gold : System.Windows.Media.Brushes.LightGray;
            if (elYashil != null) elYashil.Fill = yashil > 0 ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.LightGray;
            if (elQizil != null) elQizil.Fill = qizil > 0 ? System.Windows.Media.Brushes.IndianRed : System.Windows.Media.Brushes.LightGray;

            bool sotuvBlok = qizil > 0;
            if (brdSotuvBlockAlert != null) brdSotuvBlockAlert.Visibility = sotuvBlok ? Visibility.Visible : Visibility.Collapsed;
            if (txtSotuvBlockAlert != null) txtSotuvBlockAlert.Text = sotuvBlok ? GetSotuvBlockReason() : string.Empty;
            RefreshBlockedPartiyalar();
            if (grpBlockedPartiyalar != null) grpBlockedPartiyalar.Visibility = BlockedPartiyalar.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            RefreshSotuvOmborKunlik();
        }

        private string GetSotuvBlockReason()
        {
            var qaytgan = IshlabChiqarishlar.FirstOrDefault(x => x.Tasdiqlash == "Qaytarildi");
            if (qaytgan == null)
                return "Sotuv ochiq.";

            string izoh = string.IsNullOrWhiteSpace(qaytgan.QaytarishIzoh) ? "izoh kiritilmagan" : qaytgan.QaytarishIzoh;
            return $"Sotuv bloklangan: {qaytgan.SexNomi} / {qaytgan.YarusNomi} partiyasi ombordan qaytarilgan. Sabab: {izoh}";
        }

        private void RefreshBlockedPartiyalar()
        {
            BlockedPartiyalar.Clear();
            foreach (var item in IshlabChiqarishlar.Where(x => x.Tasdiqlash == "Qaytarildi"))
            {
                BlockedPartiyalar.Add(item);
            }
            dgBlockedPartiyalar?.Items.Refresh();
        }

        private void BtnReopenBlocked_Click(object sender, RoutedEventArgs e)
        {
            if (dgBlockedPartiyalar?.SelectedItem is TuxumModel tanlangan)
            {
                var javob = AskYesNo(
                    string.Format("{0} / {1} partiyasini 'Kutilmoqda' holatiga qaytarasizmi?", tanlangan.SexNomi, tanlangan.YarusNomi),
                    string.Format("Вернуть партию {0} / {1} в статус 'Ожидание'?", tanlangan.SexNomi, tanlangan.YarusNomi));
                if (javob != MessageBoxResult.Yes) return;

                tanlangan.Tasdiqlash = "Kutilmoqda";
                tanlangan.QaytarishIzoh = string.Empty;
                dgOmbor?.Items.Refresh();
                dgIshlabChiqarish?.Items.Refresh();
                RefreshDashboard();
                return;
            }
            ShowMsg("Avval bloklangan partiyani tanlang.", "Сначала выберите заблокированную партию.");
        }

        private void BtnResolveBlocked_Click(object sender, RoutedEventArgs e)
        {
            if (dgBlockedPartiyalar?.SelectedItem is TuxumModel tanlangan)
            {
                var javob = AskYesNo(
                    string.Format("{0} / {1} partiyasini 'Qabul qilindi' holatiga o'tkazasizmi?", tanlangan.SexNomi, tanlangan.YarusNomi),
                    string.Format("Перевести партию {0} / {1} в статус 'Принято'?", tanlangan.SexNomi, tanlangan.YarusNomi));
                if (javob != MessageBoxResult.Yes) return;

                tanlangan.Tasdiqlash = "Qabul qilindi";
                tanlangan.QaytarishIzoh = string.Empty;
                foreach (var ombor in OmborHolat)
                {
                    if (ombor.Mahsulot == "1.6 kg") { ombor.Terildi += tanlangan.W1_6; AddKirimHarakat(tanlangan.Sana, "1.6 kg", tanlangan.W1_6); }
                    else if (ombor.Mahsulot == "1.7 kg") { ombor.Terildi += tanlangan.W1_7; AddKirimHarakat(tanlangan.Sana, "1.7 kg", tanlangan.W1_7); }
                    else if (ombor.Mahsulot == "1.8 kg") { ombor.Terildi += tanlangan.W1_8; AddKirimHarakat(tanlangan.Sana, "1.8 kg", tanlangan.W1_8); }
                    else if (ombor.Mahsulot == "1.9 kg") { ombor.Terildi += tanlangan.W1_9; AddKirimHarakat(tanlangan.Sana, "1.9 kg", tanlangan.W1_9); }
                    else if (ombor.Mahsulot == "2.0 kg") { ombor.Terildi += tanlangan.W2_0; AddKirimHarakat(tanlangan.Sana, "2.0 kg", tanlangan.W2_0); }
                    else if (ombor.Mahsulot == "2.5 kg") { ombor.Terildi += tanlangan.W2_5; AddKirimHarakat(tanlangan.Sana, "2.5 kg", tanlangan.W2_5); }
                    else if (ombor.Mahsulot == "Siniq") { ombor.Terildi += tanlangan.Siniq; AddKirimHarakat(tanlangan.Sana, "Siniq", tanlangan.Siniq); }
                    else if (ombor.Mahsulot == "Paket") { ombor.Terildi += tanlangan.Paket; AddKirimHarakat(tanlangan.Sana, "Paket", tanlangan.Paket); }
                }
                dgSotuvOmborHolati?.Items.Refresh();
                dgOmbor?.Items.Refresh();
                dgIshlabChiqarish?.Items.Refresh();
                foreach (var chek in Cheklar.Where(c => c.SotuvHolati == "Sariq")) chek.OmborHolati = "Yashil";
                RefreshDashboard();
                return;
            }
            ShowMsg("Avval bloklangan partiyani tanlang.", "Сначала выберите заблокированную партию.");
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int tabIndex)) mainTabControl.SelectedIndex = tabIndex;
        }

        // Boshqa Eventlar
        private void BtnAddTuxum_Click(object sender, RoutedEventArgs e)
        {
            string sexNomi = (cmbIshlabSex?.SelectedItem as SexModel)?.Nomi;
            var tanlanganYarus = cmbIshlabYarus?.SelectedItem as YarusModel;
            string yarusNomi = tanlanganYarus?.Nomi;
            string hodim = cmbIshlabHodim?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(sexNomi) || string.IsNullOrWhiteSpace(yarusNomi))
            {
                ShowMsg("Sex va Yarusni tanlang.", "Выберите цех и ярус.");
                return;
            }
            if (string.IsNullOrWhiteSpace(hodim))
            {
                ShowMsg("Hodim nomini kiriting.", "Введите имя сотрудника.");
                return;
            }

            if (!BazaHodimlar.Any(x => string.Equals(x.FISH, hodim, StringComparison.OrdinalIgnoreCase)))
            {
                BazaHodimlar.Add(new HodimModel { FISH = hodim });
                cmbIshlabHodim?.Items.Refresh();
            }

            if (!string.Equals(tanlanganYarus?.SexNomi, sexNomi, StringComparison.OrdinalIgnoreCase))
            {
                ShowMsg("Yarus tanlangan sexga tegishli bo'lishi kerak.", "Ярус должен принадлежать выбранному цеху.");
                return;
            }

            double parrandaSoni = ParseDouble(txtIshlabParrandaSoni?.Text);
            double nobudSoni = ParseDouble(txtIshlabNobudSoni?.Text);
            double yemKg = ParseDouble(txtIshlabYemKg?.Text);
            double w16 = ParseDouble(txtIshlabW16?.Text);
            double w17 = ParseDouble(txtIshlabW17?.Text);
            double w18 = ParseDouble(txtIshlabW18?.Text);
            double w19 = ParseDouble(txtIshlabW19?.Text);
            double w20 = ParseDouble(txtIshlabW20?.Text);
            double w25 = ParseDouble(txtIshlabW25?.Text);
            double siniq = ParseDouble(txtIshlabSiniq?.Text);
            double paket = ParseDouble(txtIshlabPaket?.Text);

            var item = new TuxumModel
            {
                ID = IshlabChiqarishlar.Count + 1,
                Sana = DateTime.Now.ToString("dd.MM.yyyy"),
                SexNomi = sexNomi,
                YarusNomi = yarusNomi,
                HodimFISH = hodim,
                ParrandaSoni = parrandaSoni <= 0 ? 2000 : parrandaSoni,
                NobudSoni = nobudSoni < 0 ? 0 : nobudSoni,
                YemKg = yemKg <= 0 ? 250 : yemKg,
                W1_6 = w16 < 0 ? 0 : w16,
                W1_7 = w17 < 0 ? 0 : w17,
                W1_8 = w18 < 0 ? 0 : w18,
                W1_9 = w19 < 0 ? 0 : w19,
                W2_0 = w20 < 0 ? 0 : w20,
                W2_5 = w25 < 0 ? 0 : w25,
                Siniq = siniq < 0 ? 0 : siniq,
                Paket = paket < 0 ? 0 : paket,
                Tasdiqlash = "Kutilmoqda"
            };
            IshlabChiqarishlar.Add(item);
            txtIshlabW16.Text = "0";
            txtIshlabW17.Text = "0";
            txtIshlabW18.Text = "0";
            txtIshlabW19.Text = "0";
            txtIshlabW20.Text = "0";
            txtIshlabW25.Text = "0";
            txtIshlabSiniq.Text = "0";
            txtIshlabPaket.Text = "0";
            RefreshDashboard();
        }
        private void BtnRefreshTuxum_Click(object sender, RoutedEventArgs e)
        {
            dgIshlabChiqarish?.Items.Refresh();
            RefreshDashboard();
        }
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyOmborDateFilter();
            dgOmbor?.Items.Refresh();
            RefreshDashboard();
        }
        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (dgOmbor?.SelectedItem is TuxumModel tanlangan)
            {
                tanlangan.Tasdiqlash = "Qabul qilindi";
                foreach (var ombor in OmborHolat)
                {
                    if (ombor.Mahsulot == "1.6 kg") { ombor.Terildi += tanlangan.W1_6; AddKirimHarakat(tanlangan.Sana, "1.6 kg", tanlangan.W1_6); }
                    else if (ombor.Mahsulot == "1.7 kg") { ombor.Terildi += tanlangan.W1_7; AddKirimHarakat(tanlangan.Sana, "1.7 kg", tanlangan.W1_7); }
                    else if (ombor.Mahsulot == "1.8 kg") { ombor.Terildi += tanlangan.W1_8; AddKirimHarakat(tanlangan.Sana, "1.8 kg", tanlangan.W1_8); }
                    else if (ombor.Mahsulot == "1.9 kg") { ombor.Terildi += tanlangan.W1_9; AddKirimHarakat(tanlangan.Sana, "1.9 kg", tanlangan.W1_9); }
                    else if (ombor.Mahsulot == "2.0 kg") { ombor.Terildi += tanlangan.W2_0; AddKirimHarakat(tanlangan.Sana, "2.0 kg", tanlangan.W2_0); }
                    else if (ombor.Mahsulot == "2.5 kg") { ombor.Terildi += tanlangan.W2_5; AddKirimHarakat(tanlangan.Sana, "2.5 kg", tanlangan.W2_5); }
                    else if (ombor.Mahsulot == "Siniq") { ombor.Terildi += tanlangan.Siniq; AddKirimHarakat(tanlangan.Sana, "Siniq", tanlangan.Siniq); }
                    else if (ombor.Mahsulot == "Paket") { ombor.Terildi += tanlangan.Paket; AddKirimHarakat(tanlangan.Sana, "Paket", tanlangan.Paket); }
                }
                dgSotuvOmborHolati?.Items.Refresh();
                dgOmbor?.Items.Refresh();
                foreach (var chek in Cheklar.Where(c => c.SotuvHolati == "Sariq")) chek.OmborHolati = "Yashil";
                RefreshDashboard();
            }
        }
        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if (dgOmbor?.SelectedItem is TuxumModel tanlangan)
            {
                string izoh = PromptForRequiredText("Qaytarish izohini kiriting:");
                if (string.IsNullOrWhiteSpace(izoh)) return;
                tanlangan.Tasdiqlash = "Qaytarildi";
                tanlangan.QaytarishIzoh = izoh;
                dgOmbor?.Items.Refresh();
                RefreshDashboard();
            }
        }
        private void BtnRefreshOmbor_Click(object sender, RoutedEventArgs e)
        {
            ApplyOmborDateFilter();
            dgOmbor?.Items.Refresh();
            dgSotuvOmborHolati?.Items.Refresh();
            RefreshDashboard();
        }

        private void ApplyOmborDateFilter()
        {
            if (omborView == null) return;
            DateTime? dan = dpSanaDan?.SelectedDate?.Date;
            DateTime? gacha = dpSanaGacha?.SelectedDate?.Date;
            omborView.Filter = obj =>
            {
                var item = obj as TuxumModel;
                if (item == null) return false;
                DateTime sana = ParseDateOrToday(item.Sana);
                if (dan.HasValue && sana < dan.Value) return false;
                if (gacha.HasValue && sana > gacha.Value) return false;
                return true;
            };
            omborView.Refresh();
        }
        private void BtnToggleChekHolat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ChekModel chek)
            {
                chek.HolatYashil = !chek.HolatYashil;
                dgCheklar?.Items.Refresh();
            }
        }

        private void DpOmborKunlikSana_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => RefreshSotuvOmborKunlik();
        private void DpOmborFilterDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ApplyOmborDateFilter();
        private void TxtKassaHisob_TextChanged(object sender, TextChangedEventArgs e) => RefreshKassaHisobi();

        private void BtnKassaQabul_Click(object sender, RoutedEventArgs e)
        {
            if (dgKassaKirim?.SelectedItem is KassaKirimModel kirim)
            {
                kirim.Holat = "Qabul qilindi";
                var chek = Cheklar.FirstOrDefault(c => c.ID == kirim.ChekID);
                if (chek != null)
                {
                    chek.SotuvHolati = "Sariq";
                    chek.OmborHolati = "Yashil";
                }
                dgKassaKirim?.Items.Refresh();
                dgCheklar?.Items.Refresh();
                SyncKassaKirimlarFromCheklar();
                RefreshKassaHisobi();
                return;
            }
            ShowMsg("Kirim ro'yxatidan satr tanlang.", "Выберите строку в списке прихода.");
        }

        private void BtnKassaRashodAdd_Click(object sender, RoutedEventArgs e)
        {
            string turi = cmbKassaRashodTuri?.SelectedItem?.ToString();
            string kimga = txtKassaRashodKimga?.Text?.Trim();
            string izoh = txtKassaRashodIzoh?.Text?.Trim();
            double summa = ParseDouble(txtKassaRashodSumma?.Text);
            if (string.IsNullOrWhiteSpace(turi) || string.IsNullOrWhiteSpace(kimga) || summa <= 0)
            {
                ShowMsg("Harajat turi, kimga va summani to'g'ri kiriting.", "Заполните вид расхода, кому и сумму корректно.");
                return;
            }
            DateTime opSana = GetOperationalDate();
            KassaRashodlar.Add(new KassaRashodModel
            {
                Raqam = GenerateRkoNumber(opSana),
                Sana = opSana.ToString("dd.MM.yyyy"),
                Turi = turi,
                Kimga = kimga,
                Izoh = izoh,
                Summa = summa
            });
            if (cmbKassaRashodTuri != null && cmbKassaRashodTuri.Items.Count > 0) cmbKassaRashodTuri.SelectedIndex = 0;
            txtKassaRashodKimga?.Clear();
            txtKassaRashodIzoh?.Clear();
            txtKassaRashodSumma?.Clear();
            dgKassaRashod?.Items.Refresh();
            RefreshKassaHisobi();
        }

        private void CmbKassaRashodTuri_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshKassaKimgaOptionsByType();

        private void BtnKassaKimgaMenu_Click(object sender, RoutedEventArgs e)
        {
            RefreshKassaKimgaOptionsByType();
            if (popKassaKimgaMenu != null) popKassaKimgaMenu.IsOpen = true;
            txtPopupKassaKimgaFilter?.Focus();
        }

        private void TxtPopupKassaKimgaFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (popupKassaKimgaView == null) return;
            string kalit = txtPopupKassaKimgaFilter?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(kalit))
            {
                popupKassaKimgaView.Filter = null;
            }
            else
            {
                popupKassaKimgaView.Filter = obj =>
                {
                    if (obj is KassaKimgaOption item)
                    {
                        return (item.Nomi ?? string.Empty).IndexOf(kalit, StringComparison.OrdinalIgnoreCase) >= 0
                            || (item.Manba ?? string.Empty).IndexOf(kalit, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    return false;
                };
            }
            popupKassaKimgaView.Refresh();
        }

        private void DgPopupKassaKimga_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPopupKassaKimga?.SelectedItem is KassaKimgaOption tanlangan)
            {
                txtKassaRashodKimga.Text = tanlangan.Nomi;
                if (popKassaKimgaMenu != null) popKassaKimgaMenu.IsOpen = false;
            }
        }

        private void RefreshKassaKimgaOptionsByType()
        {
            string turi = cmbKassaRashodTuri?.SelectedItem?.ToString() ?? string.Empty;
            KassaKimgaOptions.Clear();

            if (turi == "Оплата поставщику")
            {
                foreach (var p in BazaPudratchilar.OrderBy(x => x.Nomi))
                    KassaKimgaOptions.Add(new KassaKimgaOption { Nomi = p.Nomi, Manba = "Pudratchi" });
            }
            else if (turi == "Выплата заработной платы сотруднику")
            {
                foreach (var h in BazaHodimlar.OrderBy(x => x.FISH))
                    KassaKimgaOptions.Add(new KassaKimgaOption { Nomi = h.FISH, Manba = "Hodim" });
            }
            else
            {
                foreach (var p in BazaPudratchilar.OrderBy(x => x.Nomi))
                    KassaKimgaOptions.Add(new KassaKimgaOption { Nomi = p.Nomi, Manba = "Pudratchi" });
                foreach (var m in BazaMijozlar.OrderBy(x => x.Nomi))
                    KassaKimgaOptions.Add(new KassaKimgaOption { Nomi = m.Nomi, Manba = "Mijoz" });
                foreach (var h in BazaHodimlar.OrderBy(x => x.FISH))
                    KassaKimgaOptions.Add(new KassaKimgaOption { Nomi = h.FISH, Manba = "Hodim" });
            }

            popupKassaKimgaView?.Refresh();
            if (txtPopupKassaKimgaFilter != null) txtPopupKassaKimgaFilter.Clear();
            dgPopupKassaKimga?.Items.Refresh();
        }

        private void BtnKassaRashodPrint_Click(object sender, RoutedEventArgs e)
        {
            var rashod = dgKassaRashod?.SelectedItem as KassaRashodModel;
            if (rashod == null)
            {
                ShowMsg("Print qilish uchun rashod satrini tanlang.", "Для печати выберите строку расхода.");
                return;
            }

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;

            FlowDocument doc = BuildKassaRashodOrderDocument(rashod);
            doc.PageWidth = pd.PrintableAreaWidth;
            doc.PageHeight = pd.PrintableAreaHeight;
            doc.PagePadding = new Thickness(25);
            doc.ColumnWidth = pd.PrintableAreaWidth;
            IDocumentPaginatorSource idp = doc;
            pd.PrintDocument(idp.DocumentPaginator, $"{rashod.Raqam}_{rashod.Kimga}");
        }

        private FlowDocument BuildKassaRashodOrderDocument(KassaRashodModel rashod)
        {
            var doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12
            };

            doc.Blocks.Add(new Paragraph(new Run(L("RASXOD KASSA ORDERI", "РАСХОДНЫЙ КАССОВЫЙ ОРДЕР")))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });
            doc.Blocks.Add(new Paragraph(new Run(L("Чиқим касса ордери", "Расходный кассовый ордер")))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            });

            Table rekvizitTable = new Table();
            rekvizitTable.Columns.Add(new TableColumn { Width = new GridLength(180) });
            rekvizitTable.Columns.Add(new TableColumn { Width = new GridLength(280) });
            TableRowGroup rekvizitGroup = new TableRowGroup();
            rekvizitGroup.Rows.Add(MakeRow(true, L("RKO рақами", "Номер РКО"), rashod.Raqam));
            rekvizitGroup.Rows.Add(MakeRow(true, L("Сана", "Дата"), rashod.Sana));
            rekvizitGroup.Rows.Add(MakeRow(true, L("Харажат тури", "Вид расхода"), rashod.Turi));
            rekvizitGroup.Rows.Add(MakeRow(true, L("Кимга берилди", "Кому выдано"), rashod.Kimga));
            rekvizitGroup.Rows.Add(MakeRow(true, L("Тўлов тафсилоти", "Детали платежа"), string.IsNullOrWhiteSpace(rashod.Izoh) ? "-" : rashod.Izoh));
            rekvizitTable.RowGroups.Add(rekvizitGroup);
            doc.Blocks.Add(rekvizitTable);

            doc.Blocks.Add(new Paragraph(new Run(L("Сумма", "Сумма") + $": {rashod.Summa:N0} " + L("сўм", "сум")))
            {
                Margin = new Thickness(0, 10, 0, 3),
                FontWeight = FontWeights.Bold
            });
            doc.Blocks.Add(new Paragraph(new Run(
                L("Сўз билан", "Прописью") + $": {NumberToWords((long)Math.Round(rashod.Summa), currentLanguage)} " + L("сўм", "сум")))
            {
                Margin = new Thickness(0, 0, 0, 3)
            });

            doc.Blocks.Add(new Paragraph(new Run(L(
                "Мазкур сумма юқорида кўрсатилган мақсад учун кассадан берилди.",
                "Указанная сумма выдана из кассы на указанную выше цель.")))
            {
                Margin = new Thickness(0, 10, 0, 8)
            });
            doc.Blocks.Add(new Paragraph(new Run(L("Имзолар:", "Подписи:")))
            {
                Margin = new Thickness(0, 15, 0, 5),
                FontWeight = FontWeights.SemiBold
            });
            doc.Blocks.Add(new Paragraph(new Run(L("Бухгалтер: ____________________", "Бухгалтер: ____________________"))));
            doc.Blocks.Add(new Paragraph(new Run(L("Кассир: _______________________", "Кассир: _______________________"))));
            doc.Blocks.Add(new Paragraph(new Run(L("Қабул қилувчи: _______________", "Получатель: __________________"))));

            return doc;
        }

        private string GenerateRkoNumber(DateTime opSana)
        {
            string dateKey = opSana.ToString("yyyyMMdd");
            int nextSeq = KassaRashodlar
                .Where(x => ParseDateOrToday(x.Sana) == opSana.Date)
                .Select(x => ExtractRkoSequence(x.Raqam))
                .DefaultIfEmpty(0)
                .Max() + 1;
            return $"RKO-{dateKey}-{nextSeq:000}";
        }

        private int ExtractRkoSequence(string raqam)
        {
            if (string.IsNullOrWhiteSpace(raqam)) return 0;
            string[] parts = raqam.Split('-');
            if (parts.Length < 3) return 0;
            return int.TryParse(parts[parts.Length - 1], out int seq) ? seq : 0;
        }

        private string NumberToWords(long n, string lang)
        {
            return lang == "ru" ? NumberToRussianWords(n) : NumberToUzbekWords(n);
        }

        private string NumberToUzbekWords(long n)
        {
            if (n == 0) return "nol";

            string[] birlar = { "", "bir", "ikki", "uch", "to'rt", "besh", "olti", "yetti", "sakkiz", "to'qqiz" };
            string[] onlar = { "", "o'n", "yigirma", "o'ttiz", "qirq", "ellik", "oltmish", "yetmish", "sakson", "to'qson" };

            string Read999(int x)
            {
                if (x == 0) return "";
                int yuz = x / 100;
                int on = (x / 10) % 10;
                int bir = x % 10;

                string res = "";
                if (yuz > 0) res += $"{birlar[yuz]} yuz ";
                if (on > 0) res += $"{onlar[on]} ";
                if (bir > 0) res += $"{birlar[bir]} ";
                return res.Trim();
            }

            long milliard = n / 1_000_000_000;
            long million = (n / 1_000_000) % 1_000;
            long ming = (n / 1_000) % 1_000;
            long qoldiq = n % 1_000;

            var parts = new List<string>();
            if (milliard > 0) parts.Add($"{Read999((int)milliard)} milliard");
            if (million > 0) parts.Add($"{Read999((int)million)} million");
            if (ming > 0) parts.Add($"{Read999((int)ming)} ming");
            if (qoldiq > 0) parts.Add(Read999((int)qoldiq));

            return string.Join(" ", parts).Trim();
        }

        private string NumberToRussianWords(long n)
        {
            if (n == 0) return "ноль";

            string[] onesMale = { "", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять" };
            string[] onesFemale = { "", "одна", "две", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять" };
            string[] teens = { "десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать", "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать" };
            string[] tens = { "", "", "двадцать", "тридцать", "сорок", "пятьдесят", "шестьдесят", "семьдесят", "восемьдесят", "девяносто" };
            string[] hundreds = { "", "сто", "двести", "триста", "четыреста", "пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот" };

            string Read999(int x, bool female)
            {
                if (x == 0) return "";
                int h = x / 100;
                int t = (x / 10) % 10;
                int o = x % 10;
                var partsLocal = new List<string>();
                if (h > 0) partsLocal.Add(hundreds[h]);
                if (t == 1)
                {
                    partsLocal.Add(teens[o]);
                }
                else
                {
                    if (t > 1) partsLocal.Add(tens[t]);
                    if (o > 0) partsLocal.Add((female ? onesFemale[o] : onesMale[o]));
                }
                return string.Join(" ", partsLocal);
            }

            long billions = n / 1_000_000_000;
            long millions = (n / 1_000_000) % 1_000;
            long thousands = (n / 1_000) % 1_000;
            long rest = n % 1_000;
            var result = new List<string>();

            if (billions > 0) result.Add(Read999((int)billions, false) + " " + PluralForm(billions, "миллиард", "миллиарда", "миллиардов"));
            if (millions > 0) result.Add(Read999((int)millions, false) + " " + PluralForm(millions, "миллион", "миллиона", "миллионов"));
            if (thousands > 0) result.Add(Read999((int)thousands, true) + " " + PluralForm(thousands, "тысяча", "тысячи", "тысяч"));
            if (rest > 0) result.Add(Read999((int)rest, false));

            return string.Join(" ", result).Trim();
        }

        private string PluralForm(long value, string one, string twoToFour, string many)
        {
            long n = value % 100;
            if (n >= 11 && n <= 14) return many;
            long last = value % 10;
            if (last == 1) return one;
            if (last >= 2 && last <= 4) return twoToFour;
            return many;
        }

        private void BtnKassaRashodDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgKassaRashod?.SelectedItem is KassaRashodModel rashod)
            {
                KassaRashodlar.Remove(rashod);
                dgKassaRashod?.Items.Refresh();
                RefreshKassaHisobi();
                return;
            }
            ShowMsg("O'chirish uchun rashod satrini tanlang.", "Для удаления выберите строку расхода.");
        }

        private void RefreshKassaHisobi()
        {
            DateTime targetDate = GetOperationalDate();
            double kunBoshi = ParseDouble(txtKassaKunBoshi?.Text);
            double kirim = KassaKirimlar.Where(x => x.Holat == "Qabul qilindi").Sum(x => x.Naqd);
            double rashod = KassaRashodlar.Where(x => ParseDateOrToday(x.Sana) == targetDate).Sum(x => x.Summa);
            double kunOxiri = kunBoshi + kirim - rashod;
            if (txtKassaKirimJami != null) txtKassaKirimJami.Text = kirim.ToString("N0");
            if (txtKassaRashodJami != null) txtKassaRashodJami.Text = rashod.ToString("N0");
            if (txtKassaKunOxiri != null) txtKassaKunOxiri.Text = kunOxiri.ToString("N0");
        }

        private void AddKirimHarakat(string sanaText, string mahsulot, double miqdor)
        {
            if (miqdor <= 0) return;
            KirimHarakatlar.Add(new OmborHarakatModel
            {
                Sana = ParseDateOrToday(sanaText),
                Mahsulot = mahsulot,
                Miqdor = miqdor
            });
        }

        private void RefreshSotuvOmborKunlik()
        {
            DateTime targetDate = dpOmborKunlikSana?.SelectedDate?.Date
                                  ?? dpChekSanaFiltr?.SelectedDate?.Date
                                  ?? DateTime.Today;
            string[] categories = { "1.6 kg", "1.7 kg", "1.8 kg", "1.9 kg", "2.0 kg", "2.5 kg", "Siniq", "Paket", "Nuri" };
            OmborHolat.Clear();

            foreach (string cat in categories)
            {
                double kunBoshi = KirimHarakatlar.Where(x => x.Mahsulot == cat && x.Sana < targetDate).Sum(x => x.Miqdor)
                                - SotuvHarakatlar.Where(x => x.Mahsulot == cat && x.Sana < targetDate).Sum(x => x.Miqdor);
                double kirim = KirimHarakatlar.Where(x => x.Mahsulot == cat && x.Sana == targetDate).Sum(x => x.Miqdor);
                double sotildi = SotuvHarakatlar.Where(x => x.Mahsulot == cat && x.Sana == targetDate).Sum(x => x.Miqdor);

                OmborHolat.Add(new OmborHolatiModel
                {
                    Mahsulot = cat,
                    BoshQoldiq = kunBoshi,
                    Terildi = kirim,
                    Sotildi = sotildi
                });
            }

            dgSotuvOmborHolati?.Items.Refresh();
        }
        private void BtnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dan = dpSanaDan?.SelectedDate?.Date;
            DateTime? gacha = dpSanaGacha?.SelectedDate?.Date;
            var qabulQilingan = IshlabChiqarishlar
                .Where(x => x.Tasdiqlash == "Qabul qilindi")
                .Where(x =>
                {
                    DateTime sana = ParseDateOrToday(x.Sana);
                    if (dan.HasValue && sana < dan.Value) return false;
                    if (gacha.HasValue && sana > gacha.Value) return false;
                    return true;
                })
                .ToList();

            if (!qabulQilingan.Any())
            {
                ShowMsg("Eksport uchun 'Qabul qilindi' ma'lumot topilmadi.", "Для экспорта нет данных со статусом 'Принято'.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xls)|*.xls|Excel CSV (*.csv)|*.csv",
                FileName = "TuxumOmbor_QabulQilingan.xls"
            };
            if (sfd.ShowDialog() != true) return;

            string ext = Path.GetExtension(sfd.FileName)?.ToLowerInvariant();
            if (ext == ".csv")
            {
                string csv = BuildTuxumOmborExportCsv(qabulQilingan, dan, gacha);
                File.WriteAllText(sfd.FileName, csv, new UTF8Encoding(true));
                ShowMsg("Excel (CSV) hisobot tayyorlandi.", "Excel (CSV) отчет сформирован.");
                TryOpenExportFile(sfd.FileName);
                return;
            }

            string html = BuildTuxumOmborExportHtml(qabulQilingan, dan, gacha);
            File.WriteAllText(sfd.FileName, html, new UTF8Encoding(true));
            ShowMsg("Excel (.xls) hisobot tayyorlandi.", "Excel (.xls) отчет сформирован.");
            TryOpenExportFile(sfd.FileName);
        }

        private string BuildTuxumOmborExportCsv(List<TuxumModel> items, DateTime? dan, DateTime? gacha)
        {
            var sb = new StringBuilder();
            string davr = dan.HasValue || gacha.HasValue
                ? string.Format("Davr: {0} - {1}",
                    dan?.ToString("dd.MM.yyyy") ?? "...",
                    gacha?.ToString("dd.MM.yyyy") ?? "...")
                : "Davr: barcha sanalar";

            sb.AppendLine("\"MA'LUMOTNOMA\"");
            sb.AppendLine("\"" + davr + "\"");
            sb.AppendLine(";");
            sb.AppendLine("Sex;Yarus;Melkiy(1.6);1.7;1.8;1.9;2.0;2.5;Siniq;Melanj(Paket);Jami");

            double g16 = 0, g17 = 0, g18 = 0, g19 = 0, g20 = 0, g25 = 0, gs = 0, gp = 0, gj = 0;

            foreach (var sexGroup in items.GroupBy(x => x.SexNomi).OrderBy(x => x.Key))
            {
                sb.AppendLine(string.Format("Sex;{0};;;;;;;;;;", sexGroup.Key));

                double s16 = 0, s17 = 0, s18 = 0, s19 = 0, s20 = 0, s25 = 0, ss = 0, sp = 0, sj = 0;
                var yaruslar = sexGroup
                    .GroupBy(x => x.YarusNomi)
                    .OrderBy(x => GetYarusOrder(x.Key));

                foreach (var yg in yaruslar)
                {
                    double w16 = yg.Sum(x => x.W1_6);
                    double w17 = yg.Sum(x => x.W1_7);
                    double w18 = yg.Sum(x => x.W1_8);
                    double w19 = yg.Sum(x => x.W1_9);
                    double w20 = yg.Sum(x => x.W2_0);
                    double w25 = yg.Sum(x => x.W2_5);
                    double siniq = yg.Sum(x => x.Siniq);
                    double paket = yg.Sum(x => x.Paket);
                    double jami = w16 + w17 + w18 + w19 + w20 + w25 + siniq + paket;

                    s16 += w16; s17 += w17; s18 += w18; s19 += w19; s20 += w20; s25 += w25; ss += siniq; sp += paket; sj += jami;

                    sb.AppendLine(string.Format(";{0};{1:N0};{2:N0};{3:N0};{4:N0};{5:N0};{6:N0};{7:N0};{8:N0};{9:N0}",
                        yg.Key, w16, w17, w18, w19, w20, w25, siniq, paket, jami));
                }

                sb.AppendLine(string.Format(";Jami;{0:N0};{1:N0};{2:N0};{3:N0};{4:N0};{5:N0};{6:N0};{7:N0};{8:N0}",
                    s16, s17, s18, s19, s20, s25, ss, sp, sj));
                sb.AppendLine(";");

                g16 += s16; g17 += s17; g18 += s18; g19 += s19; g20 += s20; g25 += s25; gs += ss; gp += sp; gj += sj;
            }

            sb.AppendLine(string.Format("Umumiy;Jami;{0:N0};{1:N0};{2:N0};{3:N0};{4:N0};{5:N0};{6:N0};{7:N0};{8:N0}",
                g16, g17, g18, g19, g20, g25, gs, gp, gj));
            return sb.ToString();
        }

        private void BtnPrintOmborA4_Click(object sender, RoutedEventArgs e)
        {
            DateTime? dan = dpSanaDan?.SelectedDate?.Date;
            DateTime? gacha = dpSanaGacha?.SelectedDate?.Date;
            var qabulQilingan = IshlabChiqarishlar
                .Where(x => x.Tasdiqlash == "Qabul qilindi")
                .Where(x =>
                {
                    DateTime sana = ParseDateOrToday(x.Sana);
                    if (dan.HasValue && sana < dan.Value) return false;
                    if (gacha.HasValue && sana > gacha.Value) return false;
                    return true;
                })
                .ToList();

            if (!qabulQilingan.Any())
            {
                ShowMsg("Chop etish uchun 'Qabul qilindi' ma'lumot topilmadi.", "Для печати нет данных со статусом 'Принято'.");
                return;
            }

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;
            if (pd.PrintTicket != null) pd.PrintTicket.PageOrientation = PageOrientation.Landscape;

            FlowDocument doc = BuildTuxumOmborPrintDocument(qabulQilingan, dan, gacha);
            doc.PageWidth = pd.PrintableAreaHeight > 0 ? pd.PrintableAreaHeight : 1100;
            doc.PageHeight = pd.PrintableAreaWidth > 0 ? pd.PrintableAreaWidth : 800;
            doc.PagePadding = new Thickness(24);
            doc.ColumnWidth = doc.PageWidth;
            IDocumentPaginatorSource idp = doc;
            pd.PrintDocument(idp.DocumentPaginator, "TuxumOmbor_A4");
        }

        private FlowDocument BuildTuxumOmborPrintDocument(List<TuxumModel> items, DateTime? dan, DateTime? gacha)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 12
            };
            string davr = dan.HasValue || gacha.HasValue
                ? string.Format("{0} - {1}",
                    dan?.ToString("dd.MM.yyyy") ?? "...",
                    gacha?.ToString("dd.MM.yyyy") ?? "...")
                : "barcha sanalar";
            doc.Blocks.Add(new Paragraph(new Run("\"Мухаммад Али Кувонч\" МЧЖ Паррандачилик бўлими"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 2)
            });
            doc.Blocks.Add(new Paragraph(new Run("МАЪЛУМОТНОМА"))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 2)
            });
            doc.Blocks.Add(new Paragraph(new Run("Давр: " + davr))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            Table table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(180) });
            for (int i = 0; i < 8; i++) table.Columns.Add(new TableColumn { Width = new GridLength(75) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });

            TableRowGroup group = new TableRowGroup();
            group.Rows.Add(MakeRow(true, "Цех / Ярус", "Мелкий", "1,7", "1,8", "1,9", "2,0", "2,5", "Синиқ", "Меланж", "Жами"));

            foreach (var sexGroup in items.GroupBy(x => x.SexNomi).OrderBy(x => GetSexOrder(x.Key)))
            {
                group.Rows.Add(MakeRow(true, FormatSexTitle(sexGroup.Key), "", "", "", "", "", "", "", "", ""));
                var yarusLookup = sexGroup.GroupBy(x => x.YarusNomi).ToDictionary(x => x.Key, x => x.ToList());
                for (int i = 1; i <= 5; i++)
                {
                    var rows = yarusLookup.FirstOrDefault(kv =>
                    {
                        string key = kv.Key ?? string.Empty;
                        string digits = new string(key.Where(char.IsDigit).ToArray());
                        return digits == i.ToString();
                    }).Value ?? new List<TuxumModel>();

                    double w16 = rows.Sum(x => x.W1_6);
                    double w17 = rows.Sum(x => x.W1_7);
                    double w18 = rows.Sum(x => x.W1_8);
                    double w19 = rows.Sum(x => x.W1_9);
                    double w20 = rows.Sum(x => x.W2_0);
                    double w25 = rows.Sum(x => x.W2_5);
                    double siniq = rows.Sum(x => x.Siniq);
                    double paket = rows.Sum(x => x.Paket);
                    double jami = w16 + w17 + w18 + w19 + w20 + w25 + siniq + paket;
                    group.Rows.Add(MakeRow(false, i + " Ярус", w16.ToString("N0"), w17.ToString("N0"), w18.ToString("N0"), w19.ToString("N0"), w20.ToString("N0"), w25.ToString("N0"), siniq.ToString("N0"), paket.ToString("N0"), jami.ToString("N0")));
                }
            }

            table.RowGroups.Add(group);
            doc.Blocks.Add(table);
            return doc;
        }

        private int GetYarusOrder(string yarusNomi)
        {
            if (string.IsNullOrWhiteSpace(yarusNomi)) return int.MaxValue;
            string digits = new string(yarusNomi.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int n)) return n;
            return int.MaxValue - 1;
        }

        private string BuildTuxumOmborExportHtml(List<TuxumModel> items, DateTime? dan, DateTime? gacha)
        {
            string[] categories = { "1.6 kg", "1.7 kg", "1.8 kg", "1.9 kg", "2.0 kg", "2.5 kg", "Siniq", "Paket" };
            DateTime startDate = dan ?? items.Select(x => ParseDateOrToday(x.Sana)).DefaultIfEmpty(DateTime.Today).Min();
            DateTime endDate = gacha ?? items.Select(x => ParseDateOrToday(x.Sana)).DefaultIfEmpty(DateTime.Today).Max();

            string davr = dan.HasValue || gacha.HasValue
                ? string.Format("{0} - {1}",
                    dan?.ToString("dd.MM.yyyy") ?? "...",
                    gacha?.ToString("dd.MM.yyyy") ?? "...")
                : "barcha sanalar";

            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8' />");
            sb.AppendLine("<style>");
            sb.AppendLine("table{border-collapse:collapse;font-family:Calibri,Arial;font-size:12px;}");
            sb.AppendLine("td,th{border:1px solid #444;padding:4px;text-align:center;}");
            sb.AppendLine(".title{font-size:18px;font-weight:700;border:none;padding:8px;}");
            sb.AppendLine(".subtitle{font-size:12px;border:none;padding:4px;}");
            sb.AppendLine(".org{font-size:13px;font-weight:700;border:none;padding:4px;}");
            sb.AppendLine(".head{background:#ffe95b;font-weight:700;}");
            sb.AppendLine(".sex{background:#efefef;font-weight:700;text-align:left;}");
            sb.AppendLine(".jami{background:#d9d9d9;font-weight:700;}");
            sb.AppendLine(".umumiy{background:#ffe95b;font-weight:700;}");
            sb.AppendLine(".qoldiq{background:#fff2cc;font-weight:700;}");
            sb.AppendLine(".left{text-align:left;}");
            sb.AppendLine(".yarus td{height:24px;}");
            sb.AppendLine(".section-end td{border-bottom:2px solid #222;}");
            sb.AppendLine(".section-start td{border-top:2px solid #222;}");
            sb.AppendLine(".strong td{border-top:2px solid #222;border-bottom:2px solid #222;}");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<table>");
            sb.AppendLine("<colgroup>");
            sb.AppendLine("<col style='width:170px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:75px'/>");
            sb.AppendLine("<col style='width:85px'/>");
            sb.AppendLine("<col style='width:85px'/>");
            sb.AppendLine("<col style='width:110px'/>");
            sb.AppendLine("</colgroup>");
            sb.AppendLine("<tr><td class='org' colspan='11'>\"Мухаммад Али Кувонч\" МЧЖ Паррандачилик бўлими</td></tr>");
            sb.AppendLine("<tr><td class='title' colspan='11'>МАЪЛУМОТНОМА</td></tr>");
            sb.AppendLine(string.Format("<tr><td class='subtitle' colspan='11'>Қабул қилинган тухумлар ҳисоботи. Давр: {0}</td></tr>", EscapeHtml(davr)));

            var kunBoshi = GetStockTotalsByCategories(startDate.AddDays(-1), categories);
            var kunOxiri = GetStockTotalsByCategories(endDate, categories);
            sb.AppendLine("<tr class='head strong'><td>Кун бошига қолдиқ</td><td>Мелкий</td><td>1,7 кг</td><td>1,8 кг</td><td>1,9 кг</td><td>2,0 кг</td><td>2,5 кг</td><td>Синиқ</td><td>Меланж</td><td>Жами</td><td>Сана</td></tr>");
            sb.AppendLine(string.Format(
                "<tr class='qoldiq section-end'><td class='left'>Жами</td><td>{0:N0}</td><td>{1:N0}</td><td>{2:N0}</td><td>{3:N0}</td><td>{4:N0}</td><td>{5:N0}</td><td>{6:N0}</td><td>{7:N0}</td><td>{8:N0}</td><td>{9}</td></tr>",
                kunBoshi["1.6 kg"], kunBoshi["1.7 kg"], kunBoshi["1.8 kg"], kunBoshi["1.9 kg"], kunBoshi["2.0 kg"], kunBoshi["2.5 kg"], kunBoshi["Siniq"], kunBoshi["Paket"], categories.Sum(c => kunBoshi[c]), startDate.ToString("dd.MM.yyyy")));
            sb.AppendLine("<tr class='head'><td>Цех / Ярус</td><td>Мелкий(1.6)</td><td>1,7 кг</td><td>1,8 кг</td><td>1,9 кг</td><td>2,0 кг</td><td>2,5 кг</td><td>Синиқ</td><td>Меланж</td><td>Жами</td><td>Изоҳ</td></tr>");

            double g16 = 0, g17 = 0, g18 = 0, g19 = 0, g20 = 0, g25 = 0, gs = 0, gp = 0, gj = 0;
            foreach (var sexGroup in items.GroupBy(x => x.SexNomi).OrderBy(x => GetSexOrder(x.Key)).ThenBy(x => x.Key))
            {
                sb.AppendLine(string.Format("<tr class='sex section-start'><td class='left' colspan='11'>{0}</td></tr>", EscapeHtml(FormatSexTitle(sexGroup.Key))));

                double s16 = 0, s17 = 0, s18 = 0, s19 = 0, s20 = 0, s25 = 0, ss = 0, sp = 0, sj = 0;
                var yarusLookup = sexGroup.GroupBy(x => x.YarusNomi).ToDictionary(x => x.Key, x => x.ToList());
                for (int i = 1; i <= 5; i++)
                {
                    string yarusKey = i + "-Yarus";
                    var rows = yarusLookup.ContainsKey(yarusKey) ? yarusLookup[yarusKey] : new List<TuxumModel>();
                    if (rows.Count == 0)
                    {
                        // mos nom topish (masalan "1 Yarus")
                        string alt = i + " Yarus";
                        if (yarusLookup.ContainsKey(alt)) rows = yarusLookup[alt];
                    }
                    if (rows.Count == 0)
                    {
                        var matched = yarusLookup
                            .FirstOrDefault(kv =>
                            {
                                string key = kv.Key ?? string.Empty;
                                string digits = new string(key.Where(char.IsDigit).ToArray());
                                return digits == i.ToString();
                            });
                        if (matched.Value != null) rows = matched.Value;
                    }

                    double w16 = rows.Sum(x => x.W1_6);
                    double w17 = rows.Sum(x => x.W1_7);
                    double w18 = rows.Sum(x => x.W1_8);
                    double w19 = rows.Sum(x => x.W1_9);
                    double w20 = rows.Sum(x => x.W2_0);
                    double w25 = rows.Sum(x => x.W2_5);
                    double siniq = rows.Sum(x => x.Siniq);
                    double paket = rows.Sum(x => x.Paket);
                    double jami = w16 + w17 + w18 + w19 + w20 + w25 + siniq + paket;

                    s16 += w16; s17 += w17; s18 += w18; s19 += w19; s20 += w20; s25 += w25; ss += siniq; sp += paket; sj += jami;

                    sb.AppendLine(string.Format(
                        "<tr class='yarus'><td class='left'>{0}</td><td>{1:N0}</td><td>{2:N0}</td><td>{3:N0}</td><td>{4:N0}</td><td>{5:N0}</td><td>{6:N0}</td><td>{7:N0}</td><td>{8:N0}</td><td>{9:N0}</td><td></td></tr>",
                        i + " Ярус", w16, w17, w18, w19, w20, w25, siniq, paket, jami));
                }

                sb.AppendLine(string.Format(
                    "<tr class='jami strong'><td class='left'>Жами</td><td>{0:N0}</td><td>{1:N0}</td><td>{2:N0}</td><td>{3:N0}</td><td>{4:N0}</td><td>{5:N0}</td><td>{6:N0}</td><td>{7:N0}</td><td>{8:N0}</td><td></td></tr>",
                    s16, s17, s18, s19, s20, s25, ss, sp, sj));
                sb.AppendLine("<tr><td colspan='11' style='border:none;height:8px;'></td></tr>");

                g16 += s16; g17 += s17; g18 += s18; g19 += s19; g20 += s20; g25 += s25; gs += ss; gp += sp; gj += sj;
            }

            sb.AppendLine(string.Format(
                "<tr class='umumiy strong'><td class='left'>Умумий</td><td>{0:N0}</td><td>{1:N0}</td><td>{2:N0}</td><td>{3:N0}</td><td>{4:N0}</td><td>{5:N0}</td><td>{6:N0}</td><td>{7:N0}</td><td>{8:N0}</td><td></td></tr>",
                g16, g17, g18, g19, g20, g25, gs, gp, gj));
            sb.AppendLine(string.Format(
                "<tr class='qoldiq strong'><td class='left'>Кун охирига қолдиқ</td><td>{0:N0}</td><td>{1:N0}</td><td>{2:N0}</td><td>{3:N0}</td><td>{4:N0}</td><td>{5:N0}</td><td>{6:N0}</td><td>{7:N0}</td><td>{8:N0}</td><td></td></tr>",
                kunOxiri["1.6 kg"], kunOxiri["1.7 kg"], kunOxiri["1.8 kg"], kunOxiri["1.9 kg"], kunOxiri["2.0 kg"], kunOxiri["2.5 kg"], kunOxiri["Siniq"], kunOxiri["Paket"], categories.Sum(c => kunOxiri[c])));

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private string EscapeHtml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private Dictionary<string, double> GetStockTotalsByCategories(DateTime endDateInclusive, string[] categories)
        {
            var totals = categories.ToDictionary(c => c, c => 0d);
            foreach (string cat in categories)
            {
                double kirim = KirimHarakatlar.Where(x => x.Mahsulot == cat && x.Sana <= endDateInclusive).Sum(x => x.Miqdor);
                double sotuv = SotuvHarakatlar.Where(x => x.Mahsulot == cat && x.Sana <= endDateInclusive).Sum(x => x.Miqdor);
                totals[cat] = kirim - sotuv;
            }
            return totals;
        }

        private int GetSexOrder(string sexNomi)
        {
            if (string.IsNullOrWhiteSpace(sexNomi)) return int.MaxValue;
            string digits = new string(sexNomi.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int n)) return n;
            return int.MaxValue - 1;
        }

        private string FormatSexTitle(string sexNomi)
        {
            if (string.IsNullOrWhiteSpace(sexNomi)) return "Цех № ?";
            string digits = new string(sexNomi.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrWhiteSpace(digits)) return "Цех № " + digits;
            return "Цех: " + sexNomi;
        }

        private void TryOpenExportFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch
            {
                // fayl ochish majburiy emas
            }
        }
        private void BtnAddSex_Click(object sender, RoutedEventArgs e)
        {
            string sexNomi = txtSexNomiKatalog?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(sexNomi)) return;
            if (Sexlar.Any(x => string.Equals(x.Nomi, sexNomi, StringComparison.OrdinalIgnoreCase)))
            {
                ShowMsg("Bu sex allaqachon mavjud.", "Этот цех уже существует.");
                return;
            }
            Sexlar.Add(new SexModel { Nomi = sexNomi });
            txtSexNomiKatalog?.Clear();
            cmbIshlabSex?.Items.Refresh();
            cmbJujaSex?.Items.Refresh();
            cmbYarusSexTanlash?.Items.Refresh();
            RefreshIshlabYaruslar();
            RefreshJujaYaruslar();
        }
        private void BtnAddYarus_Click(object sender, RoutedEventArgs e)
        {
            string sexNomi = (cmbYarusSexTanlash?.SelectedItem as SexModel)?.Nomi;
            string yarusNomi = txtYarusNomiKatalog?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(sexNomi) || string.IsNullOrWhiteSpace(yarusNomi))
            {
                ShowMsg("Avval sex va yarus nomini kiriting.", "Сначала укажите цех и имя яруса.");
                return;
            }
            if (Yaruslar.Any(x => x.SexNomi == sexNomi && string.Equals(x.Nomi, yarusNomi, StringComparison.OrdinalIgnoreCase)))
            {
                ShowMsg("Bu yarus shu sexda mavjud.", "Этот ярус уже есть в данном цехе.");
                return;
            }
            Yaruslar.Add(new YarusModel { SexNomi = sexNomi, Nomi = yarusNomi });
            txtYarusNomiKatalog?.Clear();
            RefreshIshlabYaruslar();
            RefreshJujaYaruslar();
        }

        private void CmbIshlabSex_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshIshlabYaruslar();
        private void CmbJujaSex_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshJujaYaruslar();

        private void BtnAddJujaKirim_Click(object sender, RoutedEventArgs e)
        {
            string sexNomi = (cmbJujaSex?.SelectedItem as SexModel)?.Nomi;
            string yarusNomi = (cmbJujaYarus?.SelectedItem as YarusModel)?.Nomi;
            if (string.IsNullOrWhiteSpace(sexNomi) || string.IsNullOrWhiteSpace(yarusNomi))
            {
                ShowMsg("Sex va Yarusni tanlang.", "Выберите цех и ярус.");
                return;
            }

            DateTime sana = dpJujaSana?.SelectedDate?.Date ?? DateTime.Today;
            double kelgan = ParseDouble(txtJujaKelganSoni?.Text);
            double kasal = ParseDouble(txtJujaKasalSoni?.Text);
            double nobud = ParseDouble(txtJujaNobudSoni?.Text);
            if (kelgan <= 0)
            {
                ShowMsg("Kelgan soni 0 dan katta bo'lishi kerak.", "Количество поступивших должно быть больше 0.");
                return;
            }

            JujaKirimlar.Add(new JujaKirimModel
            {
                Sana = sana.ToString("dd.MM.yyyy"),
                SexNomi = sexNomi,
                YarusNomi = yarusNomi,
                KelganSoni = kelgan,
                KasalSoni = kasal < 0 ? 0 : kasal,
                NobudSoni = nobud < 0 ? 0 : nobud,
                Izoh = txtJujaIzoh?.Text
            });

            txtJujaKelganSoni.Text = "0";
            txtJujaKasalSoni.Text = "0";
            txtJujaNobudSoni.Text = "0";
            txtJujaIzoh?.Clear();
            dgJujaKirimlar?.Items.Refresh();
        }

        private string PromptForRequiredText(string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 380,
                Height = 170,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            var root = new StackPanel { Margin = new Thickness(10) };
            var txt = new TextBox { Height = 28, Margin = new Thickness(0, 8, 0, 10) };
            var btns = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var ok = new Button { Content = "OK", Width = 70, Margin = new Thickness(0, 0, 6, 0) };
            var cancel = new Button { Content = "Bekor", Width = 70 };
            root.Children.Add(new TextBlock { Text = "Izoh:" });
            root.Children.Add(txt);
            btns.Children.Add(ok);
            btns.Children.Add(cancel);
            root.Children.Add(btns);
            dialog.Content = root;

            string result = null;
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    ShowMsg("Izoh majburiy.", "Комментарий обязателен.");
                    return;
                }
                result = txt.Text.Trim();
                dialog.DialogResult = true;
            };
            cancel.Click += (s, e) => dialog.DialogResult = false;
            dialog.ShowDialog();
            return result;
        }
    }
}