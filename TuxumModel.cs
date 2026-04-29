namespace Parrandachilik
{
    public class TuxumModel
    {
        public int ID { get; set; }
        public string Sana { get; set; }
        public string SexNomi { get; set; }
        public string YarusNomi { get; set; }
        public string HodimFISH { get; set; }
        public double ParrandaSoni { get; set; }
        public double NobudSoni { get; set; }
        public double YemKg { get; set; }

        // Og'irliklar
        public double W1_6 { get; set; }
        public double W1_7 { get; set; }
        public double W1_8 { get; set; }
        public double W1_9 { get; set; }
        public double W2_0 { get; set; }
        public double W2_5 { get; set; }

        // Boshqalar
        public double Siniq { get; set; }
        public double Paket { get; set; }
        public double JamiTuxum => W1_6 + W1_7 + W1_8 + W1_9 + W2_0 + W2_5 + Siniq + Paket;

        // Statuslar
        public string Tahrirlash { get; set; } = "✏️ Tahrir";
        public string Ochirish { get; set; } = "🗑️ O'chirish";
        public bool Tasdiq { get; set; } = false;
        public string Tasdiqlash { get; set; } = "Kutilmoqda";
        public string QaytarishIzoh { get; set; }
    }
}