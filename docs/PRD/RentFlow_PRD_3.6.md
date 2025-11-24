# **Ürün Gereksinim Dokümanı (PRD) – RentFlow**

Versiyon: 3.7 (Araç Başı Fiyatlandırma Modeli Detaylandırıldı)  
Tarih: 24.11.2025

## **1\) Genel Bakış (TL;DR)**

RentFlow, Türkiye’deki küçük ve orta ölçekli araç kiralama ofislerine yönelik, çok kiracılı (multi-tenant) bir SaaS platformudur. Ürünün MVP'si, ofislerin iç operasyonel verimliliğini artırmaya odaklanmıştır.

**Ana Modüller (MVP):**

1. **SaaS Admin Paneli:** Platform yönetimi için.  
2. **Landing & Self‑Signup:** Müşterilerin kendi kendine demo başlatıp ödeme yapması için.  
3. **Ofis Uygulaması (Web \- Blazor Server):** Günlük operasyonların yönetildiği ana panel.  
4. **Mobil Uygulama (React Native – Expo):** Sahada fotoğraflı ve imzalı teslim/iade tutanakları için.

PRD belgesinde "API uygulaması" ifadesi doğrudan bir "Ana Modül" olarak listelenmemiştir. Ancak, belgenin **8\) Teknik Mimari ve Gereksinimler** bölümünde ".NET 8 Backend"den bahsedilmektedir. Bu ".NET 8 Backend" ifadesi, genellikle bir API katmanını içeren sunucu tarafı uygulamayı ifade eder.

Dolayısıyla, "pi katmanı" olarak adlandırdığınız şey bir API katmanı ise, belgede bu işlevsellik "Backend" olarak ele alınmıştır. "Ana Modüller (MVP)" arasına eklemek yerine, bu mevcut modüllerin teknik gereksinimleri altında detaylandırılmış bir bileşen olarak düşünülebilir.

Eğer "API uygulaması"nı "Ana Modüller (MVP)" arasına ayrı bir madde olarak eklemek istiyorsanız, belgenin o bölümünde şu şekilde bir ekleme yapılabilir:

**Ana Modüller (MVP):**

1. **SaaS Admin Paneli:** RentFlow platformunun genel yönetimi, konfigürasyonu ve izlenmesi için tasarlanmış bir web arayüzüdür. Bu panel üzerinden, platform yöneticileri (SaaS Admin) yeni kiracılar (tenant) oluşturabilir, mevcut kiracıların abonelik planlarını yönetebilir, hesaplarını askıya alabilir ve platformun genel sağlık durumunu (aktif kiracı sayısı, aylık tekrarlayan gelir (MRR), demo dönüşüm oranı vb.) gösterge panelleri aracılığıyla izleyebilirler. Ayrıca, platform genelinde kullanılacak uygulama parametreleri ve tüm kiracılar tarafından paylaşılacak ana verilerin (araç marka/model katalogları, coğrafi veriler gibi) yönetimi de bu panelden yapılır.  
2. **Landing & Self-Signup:** Potansiyel müşterilerin RentFlow hakkında bilgi edinebilecekleri, ücretsiz demo hesabı oluşturabilecekleri ve aboneliklerini başlatıp ödeme yapabilecekleri ana web sayfası ve ilgili kayıt akışıdır. Bu modül, kullanıcıların hızlı ve self-servis bir şekilde platforma erişmesini sağlamak üzere tasarlanmıştır. Kullanıcılar, belirledikleri firma adı, e-posta ve şifre ile kayıt formunu doldurarak anında limitli bir demo kiracısı oluşturabilir ve doğrudan Ofis Web Uygulamasına yönlendirilirler.  
3. **Ofis Uygulaması (Web \- Blazor Server):** Araç kiralama ofislerinin günlük operasyonel süreçlerini (rezervasyon yönetimi, araç takibi, müşteri ilişkileri vb.) web üzerinden yönetmelerini sağlayan ana arayüzdür. Blazor Server teknolojisi ile geliştirilen bu uygulama, ofis yöneticileri ve ön masa personeli tarafından kullanılır. Araçların müsaitlik durumunu takvim üzerinden anlık olarak görüntüleme, yeni rezervasyonlar oluşturma, araç teslim (check-out) ve iade (check-in) işlemlerini tamamlama, müşteri bilgilerini yönetme gibi temel fonksiyonları barındırır. Ayrıca, ofis yöneticileri kendi personellerini ekleyip yetkilendirebilir ve ofise özel ayarları yapabilir.  
4. **Mobil Uygulama (React Native – Expo):** Sahadaki saha personelinin (araç teslimatı veya iadesi yapan çalışanlar) operasyonel görevlerini mobil cihazlar üzerinden kolayca yerine getirmesi için geliştirilmiş çapraz platform (iOS/Android) uygulamasıdır. React Native ve Expo ile geliştirilmiştir. Bu uygulama sayesinde saha personeli, kendilerine atanan teslimat işlerini görebilir, araçların dört bir yanından zorunlu fotoğraf çekebilir, kilometre ve yakıt bilgilerini girebilir ve müşterinin mobil cihaz ekranına dijital imzasını alarak fotoğraflı ve imzalı teslim/iade tutanakları (PDF) oluşturabilir. Çevrimdışı (offline) çalışma yeteneği ile internet bağlantısının olmadığı durumlarda da veri toplama ve daha sonra senkronizasyon yapabilme kabiliyetine sahiptir.  
5. **API Katmanı (Backend):** Diğer tüm modüllerin (SaaS Admin, Ofis Web Uygulaması, Mobil Uygulama) ve gelecekteki dış entegrasyonların (örneğin ödeme sistemleri, SMS servisleri vb.) veri alışverişi için kullandığı merkezi servis katmanıdır. .NET 8 teknolojisi ile geliştirilmiştir. Bu katman, tüm iş mantığını, veri erişimini ve güvenlik kontrollerini barındırır. Çoklu kiracılık (multi-tenancy) mimarisini destekleyerek her kiracının verisinin izole ve güvenli bir şekilde saklanmasını sağlar.

**Temel Değer Önerileri:**

* Takvim merkezli rezervasyon yönetimi ile çifte rezervasyonu önleme.  
* Self-servis kayıt ve demo süreci ile anında başlangıç imkanı.  
* Mobil uygulama üzerinden dijital, fotoğraflı ve imzalı tutanaklar ile hasar takibini kolaylaştırma ve anlaşmazlıkları azaltma.

## **2\) Hedefler**

### **İş Hedefleri**

* İlk 6 ay içinde **20'den fazla aktif kiracıya (tenant)** self-servis kanalıyla ulaşmak.  
* Demo'dan ücretli üyeliğe geçiş oranını **%25'in üzerinde** tutmak.  
* İlk yıl sonunda Aylık Tekrarlayan Gelir (MRR) hedefini **₺500.000** olarak belirlemek.  
* Yıllık müşteri kayıp oranını (churn) **%15'in altında** tutmak.

### **Ürün Hedefleri**

* Bir kiracının kayıt olup demo paneline erişme süresini **5 dakikanın altında** tutmak.  
* Ön büro personelinin bir rezervasyon oluşturup onaylama medyan süresini **3 dakikanın altına** indirmek.  
* Sistemdeki **çifte rezervasyon oranını %0,5'in altına** düşürmek.  
*   
  12. ay sonunda teslim/iade işlemlerinin **%80'inden fazlasının mobil uygulama üzerinden** tamamlanmasını sağlamak.

### **Kapsam Dışı Hedefler (Non-Goals for MVP)**

* Çok şubeli ofis yönetimi.  
* GPS tabanlı araç takibi veya lokasyon doğrulama.  
* Online Seyahat Acenteleri (OTA) ile entegrasyon (örn. Rentalcars, Expedia).  
* E-fatura / E-arşiv entegrasyonu.  
* Son kullanıcının (araç kiralayan müşteri) rezervasyon yapabileceği halka açık web sitesi veya mobil uygulama.  
* Gelişmiş İş Zekası (BI) ve raporlama.

## **3\) Personalar & Kullanım Hikayeleri**

### **3.1 SaaS Admin**

* **Kimdir?** RentFlow platformunun yöneticisi.  
* **Hikayeler:**  
  * Yeni bir kiracı (tenant) oluşturabilir, abonelik planını atayabilir ve hesabını askıya alabilir.  
  * Abonelik planlarını (fiyat, araç/kullanıcı limiti vb.) yönetebilir.  
  * Platformun genel sağlık durumunu (aktif kiracı sayısı, MRR, demo dönüşüm oranı) ana gösterge panelinden izleyebilir.  
  * Platform genelinde kullanılacak olan uygulama parametrelerinden sorumlu olacaktır.  
  * Platform genelinde veri bütünlüğünü ve standardizasyonunu sağlamak amacıyla, tüm kiracılar tarafından paylaşılacak olan ana verileri (master data) yönetir: Araç marka/model katalogları, coğrafi veriler (il/ilçe), genel tanımlar (para birimi, donanım özellikleri vb.).

### **3.2 Ofis Yöneticisi (Tenant Admin)**

* **Kimdir?** Araç kiralama ofisinin sahibi veya yöneticisi.  
* **Hikayeler:**  
  * Landing page üzerinden kayıt olarak firması için anında limitli bir demo hesabı başlatabilir.  
  * Demo limitlerine ulaştığında, bir üst plana geçmek için online ödeme yapabilir.  
  * Kendi personelini (ön masa, saha elemanı) sisteme ekleyebilir ve yetkilendirebilir.

### **3.3 Ön Masa Personeli (Web Kullanıcısı)**

* **Kimdir?** Ofiste telefonlara bakan, müşteri karşılayan ve rezervasyonları yöneten personel.  
* **Hikayeler:**  
  * Rezervasyon takviminden araçların müsaitlik durumunu anlık olarak görebilir ve yeni rezervasyon oluşturabilir.  
  * Müşteri geldiğinde, masaüstü web uygulamasından aracı teslim (check-out) edebilir.  
  * Araç döndüğünde, iade (check-in) işlemlerini tamamlayabilir ve ek ücretleri (gecikme, km aşımı vb.) kaydedebilir.

### **3.4 Saha Personeli (Mobil Kullanıcısı)**

* **Kimdir?** Araçları müşterinin lokasyonuna götüren veya havalimanında teslim eden personel.  
* **Hikayeler:**  
  * Mobil uygulamasından kendisine atanan teslimat işlerini görebilir.  
  * Aracı teslim ederken, zorunlu adımlarla aracın **dört bir yanından fotoğraf** çekebilir.  
  * Kilometre ve yakıt bilgilerini girip, müşterinin **imzasını tablet/telefon ekranına alarak** dijital bir teslim tutanağı (PDF) oluşturabilir.

## **4\) Kullanıcı Rolleri ve Yetki Matrisi**

Bu matris, sistemdeki farklı kullanıcı rollerinin hangi modüllere ve özelliklere ne seviyede erişebileceğini netleştirir.

| Özellik / Eylem | SaaS Admin | Ofis Yöneticisi (Admin) | Ön Masa Personeli | Saha Personeli (Mobil) |
| :---- | :---- | :---- | :---- | :---- |
| **Platform Yönetimi** |  |  |  |  |
| Tenant Yönetimi (Oluşturma, Askıya Alma) | ✔️ | \- | \- | \- |
| Abonelik Planlarını Yönetme | ✔️ | \- | \- | \- |
| Global Raporları Görüntüleme (MRR, Tenant Sayısı) | ✔️ | \- | \- | \- |
| **Ofis Yönetimi (Tenant Bazlı)** |  |  |  |  |
| Personel Ekleme/Silme/Yetkilendirme | \- | ✔️ | ❌ | ❌ |
| Ofis Ayarlarını Yönetme | \- | ✔️ | ❌ | ❌ |
| Abonelik ve Fatura Bilgilerini Görüntüleme | \- | ✔️ | ❌ | ❌ |
| **Araç Yönetimi** |  |  |  |  |
| Araç Ekleme/Silme | \- | ✔️ | ❌ | ❌ |
| Araç Güncelleme (KM, bakım durumu vb.) | \- | ✔️ | ✔️ | ❌ |
| Araç Listesini Görüntüleme | \- | ✔️ | ✔️ | ✔️ |
| **Müşteri Yönetimi (CRM)** |  |  |  |  |
| Müşteri Ekleme/Silme | \- | ✔️ | ❌ | ❌ |
| Müşteri Güncelleme/Görüntüleme | \- | ✔️ | ✔️ | ❌ |
| **Rezervasyon Yönetimi** |  |  |  |  |
| Rezervasyon Oluşturma/Güncelleme/İptal Etme | \- | ✔️ | ✔️ | ❌ |
| Rezervasyon Takvimini Görüntüleme | \- | ✔️ | ✔️ | ✔️ |
| Ofis Raporlarını Görüntüleme | \- | ✔️ | ❌ | ❌ |
| **Saha Operasyonları (Mobil)** |  |  |  |  |
| Teslim/İade Tutanağı Oluşturma | \- | ✔️ | ✔️ | ✔️ |
| Kendine Atanan İşleri Görüntüleme | \- | \- | \- | ✔️ |

**Açıklamalar:**

* ✔️: Tam Yetkili (Oluşturma, Okuma, Güncelleme, Silme gibi ilgili tüm aksiyonları kapsar)  
* ❌: Yetkisi Yok  
* \-: İlgili Değil / Uygulanamaz

## **5\) Kullanıcı Deneyimi ve Ana Akışlar**

### **5.1 Yeni Müşteri Kayıt & Demo Akışı**

1. Kullanıcı rentflow.app adresindeki "Ücretsiz Dene" butonuna tıklar.  
2. Firma adı, e-posta ve şifre ile kayıt formunu doldurur.  
3. Sistem otomatik olarak limitli bir demo kiracısı oluşturur ve kullanıcıyı doğrudan Ofis Web Uygulamasına yönlendirir.  
4. Kullanıcıyı, ilk aracını, müşterisini ve rezervasyonunu oluşturması için yönlendiren bir "İlk Kullanım Sihirbazı" karşılar.

### **5.2 Demodan Ücretli Plana Geçiş**

1. Kullanıcı demo limitlerine (örn. 3 araç, 5 rezervasyon) ulaştığında bir uyarı görür.  
2. Menüdeki "Plana Yükselt" butonu veya uyarıdaki link ile planları karşılaştırma sayfasına gider.  
3. Iyzico üzerinden güvenli ödemeyi tamamlar.  
4. Ödeme onayıyla birlikte hesabı anında **Aktif** statüsüne geçer, tüm verileri korunur ve limitler kalkar.

### **5.3 Mobil Teslim/İade Akışı**

1. Saha personeli mobil uygulamada ilgili rezervasyonu seçer.  
2. **Teslim (Check-Out):** Uygulama, fotoğraf çekme sihirbazını başlatır (ön, arka, sağ, sol zorunlu). Personel KM ve yakıt seviyesini girer. Müşteri ekrana imzasını atar. Sistem, **PDF Teslim Tutanağı** oluşturur.  
3. **İade (Check-In):** Aynı rezervasyon için yeni bir fotoğraf seti çekilir. Yeni KM ve yakıt seviyesi girilir. Yeni bir **PDF İade Tutanağı** oluşturulur.

## **6\) Bildirimler (Notifications)**

Bu bölüm, sistemdeki önemli olaylar karşısında hangi kullanıcıların, hangi kanallar üzerinden ve ne tür bildirimler alacağını tanımlar.

| Olay | Hedef Kitle | Kanal | Bildirim İçeriği (Örnek) |
| :---- | :---- | :---- | :---- |
| Yeni Demo Hesap Oluşturuldu | Ofis Yöneticisi | E-posta | "RentFlow'a Hoş Geldiniz\! Hesabınız başarıyla oluşturuldu." |
| Demodan Ücretli Plana Geçildi | Ofis Yöneticisi | E-posta | "Ödemeniz alındı. \[Paket Adı\] planınız aktifleştirildi." |
| Abonelik Yenilemesi Başarılı | Ofis Yöneticisi | E-posta | Aylık faturanın PDF eki ve teşekkür mesajı. |
| Abonelik Yenilemesi Başarısız | Ofis Yöneticisi | E-posta & In-App Uyarı | "Ödeme alınamadı. Lütfen ödeme bilgilerinizi güncelleyin." |
| Yeni Teslimat Atandı | Saha Personeli | Push Bildirimi (Mobil) | "Yeni teslimat: \[Müşteri Adı\], \[Tarih/Saat\]" |
| Parola Sıfırlama İsteği | Tüm Kullanıcılar | E-posta | Parola sıfırlama linki içeren güvenlik e-postası. |

## **7\) Veri Modeli**

### **7.1 Ana Varlıklar**
* **Tenants:** Kiracı bilgileri (şirket adı, iletişim bilgileri, durum).
* **SubscriptionPlans:** Abonelik planı tanımları (araç limitleri, fiyatlandırma).
* **SubscriptionHistory:** Tenant'ın abonelik geçmişi ve ödeme kayıtları.
* **Users:** Kullanıcı bilgileri ve rolleri.
* **Vehicles:** Araç bilgileri.
* **Customers:** Müşteri bilgileri.
* **Bookings:** Rezervasyon kayıtları.

### **7.2 Mobil Akışlar İçin Varlıklar**
* **Deliveries:** Teslim/iade işlemleri.
* **DeliveryPhotos:** Teslim/iade fotoğrafları.
* **DeliveryReports:** PDF tutanakları.

### **7.3 Araç Başı Fiyatlandırma Modeli - Veri Yapısı**

RentFlow'un fiyatlandırma modeli **araç sayısı bazlıdır**. Bu model, hem işletmeler için basit ve şeffaf bir fiyatlandırma sunar, hem de platform için detaylı iş analitiği ve gelir takibi sağlar.

#### **7.3.1 SubscriptionPlan (Plan Tanımı)**

Her abonelik planı, belirli bir araç aralığını kapsar ve ilgili fiyatlandırmayı içerir:

SubscriptionPlan 
├─ Id: Guid (Benzersiz plan kimliği) 
├─ Name: string (Örn. "Başlangıç", "Profesyonel") 
├─ MinVehicles: int (Plan için minimum araç sayısı, örn. 1) 
├─ MaxVehicles: int? (Plan için maksimum araç sayısı, örn. 5, null = sınırsız) 
├─ MonthlyPrice: decimal (Aylık plan bedeli, örn. ₺499) 
├─ AnnualPrice: decimal (Yıllık plan bedeli, genellikle indirimli) 
├─ PerVehiclePrice: decimal? (Limit aşımı için araç başı ücret, örn. ₺60) 
├─ IsDemo: bool (Demo planı mı?) 
├─ TrialDays: int (Deneme süresi gün sayısı) 
└─ IsActive: bool (Plan aktif mi?)


**Örnekler:**
- **Demo Plan:** MinVehicles=1, MaxVehicles=1, MonthlyPrice=₺0, TrialDays=7
- **Başlangıç Plan:** MinVehicles=1, MaxVehicles=5, MonthlyPrice=₺499
- **Profesyonel Plan:** MinVehicles=6, MaxVehicles=20, MonthlyPrice=₺1.499, PerVehiclePrice=₺60
  - Eğer tenant 23 araç kullanıyorsa: ₺1.499 + (3 × ₺60) = ₺1.679/ay

#### **7.3.2 SubscriptionHistory (Abonelik Geçmişi)**

Her abonelik dönemi, değişiklik veya ödeme için ayrı bir kayıt oluşturulur. Bu yapı, şu bilgileri detaylı şekilde tutar:

SubscriptionHistory 
├─ Id: Guid 
├─ TenantId: Guid (Hangi tenant'a ait?) 
├─ SubscriptionPlanId: Guid (Hangi plan?) 
├─ StartDate: DateTime (Dönem başlangıç tarihi) 
├─ EndDate: DateTime? (Dönem bitiş tarihi) 
├─ IsActive: bool (Şu an aktif dönem mi?) 
│ 
├─ VehicleCountAtStart: int (Dönem başındaki araç sayısı) ⭐ 
├─ VehicleCountAtEnd: int? (Dönem sonundaki araç sayısı) ⭐ 
│ 
├─ BasePlanPrice: decimal (Plan temel fiyatı, örn. ₺1.499) ⭐ 
├─ AdditionalVehicleCharges: decimal (Limit aşımı ücreti) ⭐ 
├─ TotalAmount: decimal (Toplam tutar = Base + Additional) ⭐ 
│ 
├─ BillingCycle: enum (Monthly, Annual) 
├─ Currency: string (Para birimi, örn. "TRY") 
├─ PaymentStatus: enum (Pending, Paid, Failed, Refunded, Cancelled) 
├─ PaymentDate: DateTime? ├─ PaymentMethod: string? 
├─ TransactionId: string? (Iyzico transaction ID) 
├─ InvoiceNumber: string? 
│ 
├─ ChangeType: enum (NewSubscription, Renewal, Upgrade, Downgrade, 
│   Cancellation, VehicleCountIncrease, VehicleCountDecrease) ⭐ 
├─ ChangeReason: string? 
├─ PreviousSubscriptionPlanId: Guid? (Önceki plan, upgrade/downgrade için) 
├─ ProratedCredit: decimal? (Önceki plandan aktarılan kredi) 
│ 
└─ CancellationDate: DateTime?


**⭐ İşaretli alanlar** araç başı fiyatlandırma için kritik öneme sahiptir.

#### **7.3.3 Neden Bu Alanlar Gerekli?**

#### **1. Faturalama ve Gelir Hesaplama**
- `BasePlanPrice` ve `AdditionalVehicleCharges` sayesinde fatura kalemleri otomatik oluşturulur.
- Örnek fatura:

Profesyonel Plan (6-20 araç)     ₺1.499,00 Ek Araç Ücreti (3 araç × ₺60)     ₺180,00 KDV (%20)                          ₺335,80 ───────────────────────────────────────── Toplam                           ₺2.014,80


#### **2. Müşteri Davranışı ve Büyüme Analizi**
- `VehicleCountAtStart` ve `VehicleCountAtEnd` ile tenant'ın büyüme hızı izlenir.
- Örnek: 3 araçtan 23 araca 5 ayda büyümüş → Aylık 4 araç büyüme = Sağlıklı müşteri.
- Araç sayısı düşen tenant'lar **churn riski** taşır ve proaktif müdahale gerektirir.

#### **3. Prorated (Günlük) Hesaplama**
- Tenant ayın ortasında plan değiştirdiğinde, kalan günler için adil fiyatlandırma yapılır.
- `ProratedCredit` alanı, eski plandan kalan tutarı yeni plana aktarır.
- Örnek: 15 gün kala Başlangıç'tan Profesyonel'e geçiş → Eski plandan ₺249 kredi.

#### **4. SaaS Business Metrics**
- **MRR (Monthly Recurring Revenue):** `Sum(TotalAmount)` where `IsActive = true`
- **Expansion MRR:** `Sum(AdditionalVehicleCharges)` - Mevcut müşterilerden gelen büyüme
- **ARPU (Average Revenue Per Unit):** `TotalAmount / VehicleCountAtStart` - Araç başına ortalama gelir
- **Churn Prediction:** `VehicleCountAtEnd < VehicleCountAtStart` - Daralma riski

#### **5. Raporlama Örnekleri**

CEO Dashboard (Ocak 2025):
📊 Gelir Raporu 
├─ Toplam Gelir: ₺125.400 
├─ Base Plan Geliri: ₺98.500 (78%) 
├─ Overflow Geliri: ₺26.900 (22%) ← Büyüme göstergesi! 
├─ Toplam Araç: 1.847 araç 
└─ ARPU: ₺67,9/araç

### **7.4 İş Kuralları**

* **Çakışma Önleme:** Aynı araç için zaman aralıkları çakışamaz.  
* **Demo Limitleri:** Demo hesabı yalnızca 1 araç ekleyebilir ve 7 gün kullanabilir.  
* **Araç Limiti Kontrolü:** Tenant, planının `MaxVehicles` limitini aşan araç ekleyemez (ya da `PerVehiclePrice` uygulanır).
* **Fotoğraf Zorunluluğu:** Teslim/iade için 4 zorunlu açıdan fotoğraf çekilmelidir.  
* **Bakım Durumu:** 'Bakımda' (Maintenance) olan bir araç için yeni rezervasyon oluşturulamaz.  
* **Multi-Tenancy:** Her kullanıcı sadece kendi kiracısının (tenant) verisine erişebilir.
* **Abonelik Geçmişi:** Bir tenant için aynı anda yalnızca bir `SubscriptionHistory` kaydı `IsActive=true` olabilir.

## **8\) Teknik Mimari ve Gereksinimler**

* **Genel Yaklaşım:** .NET 8 Backend, Azure SQL Veritabanı, Blazor Server Web, React Native Mobil, ASP.NET Identity.  
* **Çoklu Kiracılık (Multi-tenancy):** Kiracı bağlamı, kullanıcının kimlik doğrulama işlemi sırasında hesabıyla ilişkili TenantId bilgisinden alınacaktır. EF Core Global Query Filter, tüm sorguları bu TenantId'ye göre otomatik olarak filtreleyecektir.

## **9\) Tasarım Sistemi ve Kullanıcı Arayüzü İlkeleri**

* **Hedef:** Web (Blazor) ve Mobil (React Native) uygulamaları arasında görsel ve deneyimsel tutarlılık sağlamak.  
* **Görsel Kimlik:** Profesyonel, temiz ve modern bir tasarım dili benimsenecektir. Ana renk paleti güveni temsil eden mavi tonları ve başarı/onay durumları için yeşil vurgular içerecektir. Okunabilirlik için platformlarda standart olan modern ve sans-serif bir yazı tipi (örn. Inter) kullanılacaktır.  
* **UI Tutarlılığı:** Düğmeler, form elemanları, uyarı mesajları, tablolar ve modal pencereler gibi temel arayüz bileşenleri için ortak bir stil kılavuzu oluşturulacaktır. Bu, kullanıcıların farklı platformlar arasında geçiş yaparken yabancılık çekmemesini sağlayacaktır.  
* **MVP Yaklaşımı:** MVP için tam kapsamlı bir tasarım sistemi oluşturulmayacak, ancak geliştirme sürecine rehberlik edecek temel bir stil kılavuzu (renk kodları, yazı tipi boyutları, temel bileşen stilleri) hazırlanacaktır.

## **10\) Uygulama İçi Metinler ve Çeviri Hazırlığı (L10n)**

* **Metin Yönetimi:** Kullanıcı arayüzünde gösterilen hiçbir metin (düğme etiketleri, başlıklar, hata mesajları vb.) kodun içine doğrudan yazılmayacaktır (hard-coded). Tüm metinler, her platform için özel kaynak dosyalarında (örn. .NET için .resx, React Native için json) yönetilecektir.  
* **MVP Dili:** Uygulamanın MVP sürümü için varsayılan ve tek dil **Türkçe** olacaktır.  
* **Geleceğe Hazırlık:** Altyapı, gelecekte yeni dillerin eklenmesini kolaylaştıracak şekilde kurgulanmalıdır. Yeni bir dil eklemek, kodda değişiklik yapmayı gerektirmemeli, sadece yeni bir kaynak dosyası eklenerek mümkün olmalıdır. Bu, projenin en başından itibaren uyulması gereken bir teknik prensiptir.

## **11\) Başarı Metrikleri**

* **Aktivasyon:** Self-signup → demo panele ulaşma süresi (p50 \< 5 dk).  
* **Dönüşüm:** Demo → ücretli plana geçiş oranı (≥ %25).  
* **Etkileşim:** Mobil teslim/iade kullanım oranı (12. ay sonunda ≥ %80).  
* **Ürün Kalitesi:** Çifte rezervasyon hata oranı (\< %0,5).  
* **Verimlilik:** Ön masa rezervasyon onay süresi (p50 \< 3 dk).  
* **Kullanıcı Memnuniyeti (CSAT/NPS):** Mobil uygulama akışı sonrası anket.  
* **Karşı Metrik:** Rezervasyon Değişiklik/İptal Oranı.

### **11.1 Analitik ve Olay Takibi (Analytics & Event Tracking)**

Başarı metriklerini doğru ölçebilmek için aşağıdaki temel kullanıcı olayları (events) bir analitik aracı (örn. Application Insights, Mixpanel) üzerinden takip edilmelidir:

* **Kayıt Akışı:**  
  * landing\_page\_viewed  
  * signup\_form\_submitted  
  * demo\_account\_created  
* **Dönüşüm Akışı:**  
  * upgrade\_plan\_page\_viewed  
  * payment\_successful  
  * plan\_upgraded  
* **Temel Ürün Kullanımı:**  
  * user\_login  
  * vehicle\_created  
  * customer\_created  
  * booking\_created  
  * mobile\_report\_submitted (Teslim veya iade tutanağı gönderildi)

## **12\) Fiyatlandırma**

### **12.1 Fiyatlandırma Tablosu**

┌─────────────────┬─────────────┬──────────────┬─────────────────┐ 
│ Paket           │ Araç Sayısı │ Aylık Fiyat  │ Araç Başı Fiyat │ 
├─────────────────┼─────────────┼──────────────┼─────────────────┤ 
│ 🚗 Demo         │ 1 araç      │ ₺0 (7 gün)   │ -               │ 
│ 🚙 Başlangıç    │ 1-5 araç    │ ₺499         │ ~₺100/araç      │ 
│ 🚐 Profesyonel  │ 6-20 araç   │ ₺1.499       │ ~₺75/araç       │ 
│ 🚌 Kurumsal     │ 21-50 araç  │ ₺2.999       │ ~₺60/araç       │ 
│ 🏢 Enterprise   │ 50+ araç    │ Özel Fiyat   │ Negotiate       │ 
└─────────────────┴─────────────┴──────────────┴─────────────────┘


### **12.2 Fiyatlandırma Mantığı**

**Temel İlkeler:**
1. **Basitlik:** Kullanıcılar "kaç aracım var = ne kadar ödüyorum" hesabını kolayca yapabilir.
2. **Volume Discount:** Araç sayısı arttıkça araç başına düşen maliyet azalır.
3. **Esneklik:** Kullanıcı sayısına limit yok, sadece araç sayısı önemli.
4. **Büyüme Teşviki:** Küçük ofisler düşük başlangıç maliyetiyle başlar, büyüdükçe doğal olarak plan yükseltir.

**Limit Aşımı Politikası:**
- **Profesyonel Plan** örneği: 6-20 araç için ₺1.499/ay. Eğer tenant 23 araç kullanıyorsa:
  - Base Plan: ₺1.499
  - Ek 3 araç: 3 × ₺60 = ₺180
  - **Toplam: ₺1.679/ay**
- Bu politika sayesinde tenant, hemen plan değiştirmek zorunda kalmadan geçici büyüme dönemlerinde esneklik kazanır.

**Yıllık Abonelik İndirimi:**
- Tüm planlar için yıllık ödemede **%17 indirim** uygulanır.
- Örnek: Profesyonel Plan aylık ₺1.499 × 12 = ₺17.988 → Yıllık ₺14.990 (₺3.000 tasarruf)

### **12.3 Plan Değişikliği Senaryoları**

**Senaryo 1: Demo → Başlangıç (3 araç)**

Durum: Demo süresi bitti veya 2. araç eklenmeye çalışılıyor 
Aksiyon: "Başlangıç planına geçin, 5 araca kadar ₺499/ay" 
Sonuç: Ödeme başarılı → Tenant.Status = Active, CurrentVehicleCount = 3

**Senaryo 2: Başlangıç → Profesyonel (6. araç ekleniyor)**

Durum: 5 araçlık limitte, kullanıcı 6. aracı eklemek istiyor 
Aksiyon: "Profesyonel plana yükseltin, 20 araca kadar ₺1.499/ay" 
Prorated: Kalan 15 gün için eski plandan ₺249 kredi Ödeme: (₺1.499/30 × 15) - ₺249 = ₺500 
Sonuç: SubscriptionHistory kaydı oluşturulur (ChangeType=Upgrade)

**Senaryo 3: Profesyonel → Limit Aşımı (23 araç)**

Durum: 20 araçlık limitte, kullanıcı 21-23 arası araç ekliyor 
Aksiyon: Otomatik ek ücret hesaplanır Hesaplama: Base ₺1.499 + (3 × ₺60) = ₺1.679/ay 
Sonuç: Bir sonraki fatura döneminde yeni tutar yansır 
Bildirim: "Limit aşımı nedeniyle ek ücret uygulanacak. Kurumsal plana geçerek tasarruf edebilirsiniz."


### **12.4 İş Avantajları**

**Müşteri Açısından:**
- ✅ Net ve anlaşılır fiyatlandırma
- ✅ Kullanıcı sayısı sınırı yok
- ✅ Küçük başlangıç maliyeti (₺499)
- ✅ Büyüdükçe araç başına maliyet düşer

**Platform Açısından:**
- ✅ Öngörülebilir gelir (MRR)
- ✅ Upsell fırsatları (araç sayısı artışı)
- ✅ Churn analizi (araç sayısı azalması = risk)
- ✅ Expansion MRR takibi (mevcut müşteri büyümesi)

┌─────────────────┬─────────────┬──────────────┬─────────────────┐
│ Paket           │ Araç Sayısı │ Aylık Fiyat  │ Araç Başı Fiyat │
├─────────────────┼─────────────┼──────────────┼─────────────────┤
│ 🚗 Demo         │ 1 araç      │ ₺0 (7 gün)   │ -               │
│ 🚙 Başlangıç    │ 1-5 araç    │ ₺499         │ ~₺100/araç      │
│ 🚐 Profesyonel  │ 6-20 araç   │ ₺1.499       │ ~₺75/araç       │
│ 🚌 Kurumsal     │ 21-50 araç  │ ₺2.999       │ ~₺60/araç       │
│ 🏢 Enterprise   │ 50+ araç    │ Özel Fiyat   │ Negotiate       │
└─────────────────┴─────────────┴──────────────┴─────────────────┘

## **13\) Riskler ve Azaltma Stratejileri**

* **Teknik Riskler:** Mobil uygulamanın çevrimdışı (offline) senkronizasyonunda yaşanabilecek veri kayıpları.  
  * **Azaltma:** Sağlam bir "background sync" kuyruğu kurmak, otomatik yeniden deneme mantığı eklemek ve veri çakışmaları için net bir çözüm stratejisi belirlemek.  
* **Pazar Riskleri:** Hedef kitlenin dijital dönüşüme direnç göstermesi.  
  * **Azaltma:** Çok basit bir "İlk Kullanım Sihirbazı" sunmak, kısa video eğitimler hazırlamak ve ürünün getireceği avantajları vaka çalışmalarıyla pazarlamak.  
* **Operasyonel Riskler:** Iyzico gibi üçüncü parti servislerde yaşanabilecek kesintiler.  
  * **Azaltma:** Kullanıcıyı uygulama içinde bilgilendirmek, durumu izlemek için proaktif monitörler kurmak ve kullanıcıyı tekrar denemesi için yönlendirmek.

## **14\) Lansman Planı (Go-to-Market)**

* **Beta Lansmanı (Kapalı \- İlk 2 Ay):** Seçilmiş 5-10 pilot ofis ile ürünü test etmek, detaylı geri bildirim toplamak ve kritik hataları gidermek.  
* **Public Lansman (Açık \- 3\. Aydan İtibaren):** Hedefli dijital pazarlama kampanyaları ve "İlk 3 ay %50 indirim" gibi teşviklerle erken kullanıcıları çekmek. Lansman döneminde canlı destek kanallarını aktif tutmak.

## **15\) Destek ve Müşteri İletişimi**

* **Destek Kanalları (MVP):** MVP aşamasında müşteriler için birincil destek kanalı e-posta olacaktır. destek@rentflow.app adresine gönderilen tüm talepler takip edilecektir. Uygulama içinde (hem web hem mobil) "Yardım & Destek" menüsü altında bu e-posta adresine yönlendiren bir iletişim formu bulunacaktır.  
* **Hizmet Seviyesi (SLA \- MVP):** Gelen tüm destek taleplerine **24 iş saati içinde** ilk yanıtın verilmesi hedeflenmektedir. Bu süreç başlangıçta manuel olarak yönetilecektir.  
* **Hata Raporlama:** Kullanıcılar, buldukları hataları (bug) yine aynı destek kanalı üzerinden bildirebilirler. İletişim formunda, kullanıcılardan sorunu daha iyi anlayabilmek için ekran görüntüsü eklemeleri ve hatayı yeniden oluşturmak için adımları detaylandırmaları istenecektir.

## **16\) Açık Sorular ve Varsayımlar**

* **Varsayımlar:**  
  * Küçük kiralama ofislerinin verimliliği artıracak bir çözüme açık olduğunu varsayıyoruz.  
  * Saha personelinin temel düzeyde akıllı telefon kullanma becerisine sahip olduğunu varsayıyoruz.  
* **Açık Sorular:**  
  * **Hukuki Soru:** Mobil cihaz üzerinden alınan dijital imza, yasal olarak ıslak imza kadar geçerli midir? (Bir hukuk danışmanıyla netleştirilmeli.)  
  * **Ürün Sorusu:** Demo sürümünün limitleri, potansiyel müşterinin ürünün değerini anlaması için yeterli mi?

## **17\) Kabul Kriterleri (Örnekler)**

* **Çakışma Önleme:** Aynı araç için kesişen bir tarih aralığında ikinci bir rezervasyon oluşturma denemesi, kullanıcıya "Bu araç seçilen tarihlerde dolu." şeklinde net bir hata mesajı göstermelidir.  
* **Demo Limitleri:** Demo hesabındaki bir kullanıcı 4\. aracını eklemeye çalıştığında, "Demo limitinize ulaştınız. Devam etmek için lütfen bir üst plana geçin." uyarısı ve plan sayfasına bir link gösterilmelidir.  
* **Mobil Fotoğraf Zorunluluğu:** Saha personeli, teslim/iade sırasında 4 zorunlu fotoğrafı çekmeden "Tutanak Oluştur" butonuna basamaz (buton pasif olmalıdır).  
* **PDF Tutanak Oluşturma:** Mobil uygulamada "Tutanak Oluştur" butonuna basıldıktan sonra 5 saniye içinde PDF'in oluşturulup sunucuya yüklenmesi ve DeliveryReports.PdfUrl alanına kaydedilmesi gerekir.  
* **Performans:** Ofis web uygulamasındaki rezervasyon takvimi, 100 araç ve 500 rezervasyon verisiyle bile 1.5 saniyenin altında yüklenmelidir.

## **18\) Hata Yönetimi ve Uç Durumlar (Error Handling & Edge Cases)**

Bu bölüm, beklenmedik durumlar veya hatalar karşısında sistemin nasıl davranması gerektiğini tanımlar.

* **Ödeme Hataları:**  
  * **Senaryo:** Kullanıcı Iyzico ödeme sayfasında işlemi iptal eder veya kart bilgileri geçersiz olduğu için ödeme başarısız olur.  
  * **Beklenen Davranış:** Kullanıcı, "Ödeme işlemi tamamlanamadı. Lütfen tekrar deneyin veya farklı bir kart kullanın." mesajıyla birlikte plan seçim sayfasına geri yönlendirilir. Kullanıcının hesabı Demo statüsünde kalmaya devam eder.  
* **Mobil Bağlantı Hataları:**  
  * **Senaryo:** Saha personeli fotoğraf ve imzaları aldıktan sonra tutanağı göndermeye çalışırken internet bağlantısı kopar.  
  * **Beklenen Davranış:** Mobil uygulama verileri (fotoğraflar, form bilgileri) cihazda güvenli bir şekilde sıraya alır. Kullanıcıya "İnternet bağlantısı yok. Bilgileriniz kaydedildi ve bağlantı kurulduğunda otomatik olarak gönderilecek." şeklinde bir bildirim gösterilir. Uygulama, periyodik olarak bağlantıyı kontrol eder ve bağlantı geldiğinde sıradaki verileri göndermeye devam eder.  
* **Veri Çakışmaları:**  
  * **Senaryo:** İki farklı ön masa personeli, aynı anda, aynı araç için, kesişmeyen ama birbirine çok yakın zaman dilimlerinde rezervasyon oluşturmaya çalışır.  
  * **Beklenen Davranış:** Veritabanı seviyesinde "ilk gelen kazanır" prensibi uygulanır. İkinci kaydetme denemesini yapan kullanıcı, "Bu araç için siz işlem yaparken yeni bir rezervasyon oluşturuldu. Lütfen takvimi kontrol edip tekrar deneyin." uyarısını alır.

## **19\) Veri Taşıma Stratejisi (Data Migration)**

* **MVP Yaklaşımı:** MVP aşamasında, yeni müşterilerin mevcut verilerini (araç listesi, müşteri listesi vb.) sisteme aktarmak için otomatik bir araç sunulmayacaktır. Veri girişi, kullanıcılar tarafından **manuel olarak** yapılacaktır.  
* **Gelecek Fazlar İçin Not:** Müşteri geri bildirimlerine ve talebe bağlı olarak, Excel/CSV formatındaki dosyalardan toplu veri içeri aktarma (import) özelliği geliştirilebilir. Onboarding sürecini kolaylaştırmak için bu özellik yol haritasında önceliklendirilebilir.

## **20\) MVP Geliştirme Fazları (Öneri)**

* **Faz 1: Altyapı ve SaaS Yönetimi (İlk 4 Hafta):**  
  * SaaS Admin paneli (tenant ve plan yönetimi).  
  * Landing page ve Self-Signup akışı (otomatik demo tenant oluşturma).  
  * Kimlik doğrulama ve yetkilendirme altyapısı.  
* **Faz 2: Ofis Operasyonları \- Web (Sonraki 4 Hafta):**  
  * Ofis Web Uygulaması: Araç, Müşteri ve Rezervasyon yönetimi.  
  * Ana rezervasyon takvimi ve çakışma önleme mantığı.  
  * Iyzico entegrasyonu ile demodan ücretli plana geçiş.  
* **Faz 3: Saha Operasyonları \- Mobil (Sonraki 4 Hafta):**  
  * Mobil Uygulama (React Native): Temel rezervasyon listeleme.  
  * Fotoğraflı ve imzalı Teslim (Check-Out) ve İade (Check-In) akışları.  
  * PDF tutanak oluşturma ve sunucuya yükleme.  
  * Çevrimdışı (offline) çalışma için temel hazırlıklar.

## **21\) Bağımlılıklar ve Entegrasyonlar**

* **Iyzico:** Abonelik ödemeleri için kritik dış bağımlılık. API anahtarlarının ve test ortamının hazır olması gerekiyor.  
* **Azure:** Tüm altyapı (App Service, SQL, Blob Storage) burada barındırılacak. Gerekli servislerin oluşturulması ve konfigürasyonu gerekiyor.  
* **Expo (EAS):** Mobil uygulamanın build ve dağıtım süreçleri için kullanılacak. Geliştirici hesaplarının ve ayarlarının yapılması gerekiyor.

## **22\) Yol Haritası (MVP Sonrası Olası Adımlar)**

1. **MVP Lansmanı:** Bu dokümanda tanımlanan tüm özellikler.  
2. **Faz 2 \- Temel Raporlama:** Gelir, doluluk gibi temel raporların oluşturulması.  
3. **Faz 3 \- AI Destekli Hasar Tespiti:** Fotoğrafları karşılaştırarak hasarları otomatik işaretleyen yapay zeka modülü.  
4. **Faz 4 \- Finansal Entegrasyonlar:** Muhasebe programları ve **E-fatura / E-arşiv** sistemleri ile entegrasyon.  
5. **Faz 5 \- Büyüme Motoru: Son Kullanıcıya Ulaşım:** Son kullanıcıların rezervasyon yapabileceği **halka açık bir web sitesi ve mobil uygulamanın** geliştirilmesi.  
6. **Faz 6 \- Gelişmiş İş Zekası (BI):** Trend analizi, talep tahmini gibi **gelişmiş BI ve raporlama** yeteneklerinin sunulması.

**Değişiklik Geçmişi:**
- **v3.7 (24.11.2025):** Araç başı fiyatlandırma modelinin veri yapısı detaylandırıldı. Bölüm 7.3 ve Bölüm 12 genişletildi. SubscriptionHistory alanlarının iş değeri açıklandı.
- **v3.6 (20.09.2025):** Operasyonel detaylar eklendi.