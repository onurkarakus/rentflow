# RentFlow – Copilot Context (PRD 3.6 Tabanlı)

**Amaç:** Bu dosya, Visual Studio 2022 GitHub Copilot Chat'in RentFlow projesini tam olarak anlaması ve bağlam-duyarlı öneriler sunması için hazırlanmıştır.

---

## 1. Proje Kimliği ve Vizyon

**RentFlow nedir?**  
Türkiye'deki küçük ve orta ölçekli araç kiralama ofislerine yönelik, çok kiracılı (multi-tenant) bir SaaS platformudur. MVP, iç operasyonel verimliliği artırmaya odaklanır; OTA entegrasyonları, GPS takibi, e-fatura veya son kullanıcıya açık rezervasyon kapsam dışıdır.

**Temel Değer Önerileri:**
1. **Çifte rezervasyon önleme:** Takvim merkezli, veritabanı constraint'leriyle desteklenen çakışma kontrolü.
2. **Self-servis onboarding:** 5 dakikanın altında demo hesap aktivasyonu.
3. **Dijital saha operasyonları:** Mobil cihazdan fotoğraflı ve imzalı teslim/iade tutanakları (PDF).

**İş Hedefleri (İlk 6-12 Ay):**
- 20+ aktif tenant (demo → ücretli dönüşüm ≥%25).
- MRR: ₺500.000 (12. ay sonunda).
- Churn: <%15.
- Mobil kullanım oranı: ≥%80 (teslim/iade işlemlerinde).

---

## 2. Mimari Prensipleri ve Teknoloji Yığını

### 2.1 Genel Mimari
- **Backend:** .NET 8 (LTS), ASP.NET Core Web API + Blazor Server.
- **Veritabanı:** Azure SQL Database (tek veritabanı, multi-tenant discriminator: `TenantId`).
- **Kimlik Doğrulama:** ASP.NET Core Identity (cookie-based auth for Blazor, JWT for mobile API).
- **Dosya Depolama:** Azure Blob Storage (teslim/iade fotoğrafları, PDF tutanakları).
- **Ödeme:** Iyzico REST API (abonelik ödemeleri).
- **Mobil:** React Native (Expo) – iOS + Android.
- **Deployment:** Azure App Service (Web + API), Azure SQL, Application Insights (telemetri).

### 2.2 Multi-Tenancy Stratejisi
**Kritik Kural:** Tüm varlıklar (Vehicle, Booking, Customer, Delivery vb.) `TenantId` sütunu içerir.

**Uygulanış:**
1. **Authentication:** Kullanıcı giriş yaptığında, `User.TenantId` claim olarak JWT/Cookie'ye eklenir.
2. **EF Core Global Query Filter:** Her `DbContext.OnModelCreating` içinde:
   ```csharp
   builder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == _currentTenantId);
   ```
   Burada `_currentTenantId`, HTTP context'ten (`HttpContext.User.FindFirst("TenantId")`) alınır ve `ITenantProvider` servisi ile inject edilir.
3. **Veri Ekleme:** Yeni bir entity kaydederken `TenantId` otomatik olarak `SaveChanges` interceptor'ü tarafından set edilir.
4. **SaaS Admin İstisnası:** SaaS Admin kullanıcıları (`IsSaaSAdmin=true` claim) için global filter devre dışı bırakılabilir (örn. `IgnoreQueryFilters()` ile).

**Güvenlik:**
- Tenant ID asla client'tan alınmamalı; her zaman sunucu tarafında auth claim'den okunmalıdır.
- Authorization policy'ler, kullanıcının yalnızca kendi tenant'ına ait kaynaklara erişebilmesini zorunlu kılar (custom `TenantResourceAuthorizationHandler`).

### 2.3 Katmanlı Mimari (Clean Architecture)

```
RentFlow.sln
├── src/
│   ├── RentFlow.Domain/              # Domain entities, value objects, domain services
│   │   ├── Entities/
│   │   │   ├── Tenant.cs
│   │   │   ├── User.cs
│   │   │   ├── Vehicle.cs
│   │   │   ├── Booking.cs
│   │   │   ├── Customer.cs
│   │   │   ├── Delivery.cs
│   │   │   └── DeliveryReport.cs
│   │   ├── ValueObjects/
│   │   │   ├── DateRange.cs
│   │   │   └── VehicleStatus.cs
│   │   ├── DomainServices/
│   │   │   └── BookingConflictChecker.cs
│   │   └── Interfaces/
│   │       └── IBookingRepository.cs
│   │
│   ├── RentFlow.Application/         # Use cases, DTOs, validation
│   │   ├── UseCases/
│   │   │   ├── Bookings/
│   │   │   │   ├── CreateBookingCommand.cs
│   │   │   │   └── CreateBookingCommandHandler.cs
│   │   │   ├── Deliveries/
│   │   │   │   ├── CompleteCheckOutCommand.cs
│   │   │   │   └── GenerateDeliveryReportPdf.cs
│   │   │   └── Tenants/
│   │   │       ├── CreateDemoTenantCommand.cs
│   │   │       └── UpgradeTenantPlanCommand.cs
│   │   ├── DTOs/
│   │   │   ├── BookingDto.cs
│   │   │   └── DeliveryDto.cs
│   │   ├── Validators/
│   │   │   └── CreateBookingCommandValidator.cs (FluentValidation)
│   │   └── Services/
│   │       ├── IPdfGenerationService.cs
│   │       └── IPaymentService.cs
│   │
│   ├── RentFlow.Infrastructure/      # EF Core, external integrations
│   │   ├── Persistence/
│   │   │   ├── RentFlowDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── BookingConfiguration.cs
│   │   │   │   └── VehicleConfiguration.cs
│   │   │   ├── Migrations/
│   │   │   ├── Interceptors/
│   │   │   │   └── TenantIdInterceptor.cs
│   │   │   └── Repositories/
│   │   │       └── BookingRepository.cs
│   │   ├── ExternalServices/
│   │   │   ├── IyzicoPaymentService.cs
│   │   │   ├── AzureBlobStorageService.cs
│   │   │   └── PdfGenerationService.cs (iTextSharp/QuestPDF)
│   │   └── Identity/
│   │       └── ApplicationUser.cs (extends IdentityUser)
│   │
│   ├── RentFlow.WebAPI/              # REST API for mobile
│   │   ├── Controllers/
│   │   │   ├── DeliveriesController.cs
│   │   │   ├── BookingsController.cs
│   │   │   └── AuthController.cs
│   │   ├── Middleware/
│   │   │   ├── TenantResolutionMiddleware.cs
│   │   │   └── GlobalExceptionHandler.cs
│   │   └── Program.cs
│   │
│   ├── RentFlow.BlazorApp/           # Blazor Server (Ofis Web + SaaS Admin)
│   │   ├── Pages/
│   │   │   ├── Bookings/
│   │   │   │   ├── Calendar.razor
│   │   │   │   └── CreateBooking.razor
│   │   │   ├── Vehicles/
│   │   │   │   └── VehicleList.razor
│   │   │   └── Admin/
│   │   │       └── TenantManagement.razor
│   │   ├── Shared/
│   │   │   ├── MainLayout.razor
│   │   │   └── TenantSwitcher.razor (SaaS Admin için)
│   │   └── Program.cs
│   │
│   └── RentFlow.Mobile/              # React Native (Expo)
│       ├── src/
│       │   ├── screens/
│       │   │   ├── DeliveryListScreen.tsx
│       │   │   └── CheckOutScreen.tsx
│       │   ├── components/
│       │   │   ├── PhotoCapture.tsx
│       │   │   └── SignaturePad.tsx
│       │   ├── services/
│       │   │   ├── apiClient.ts
│       │   │   └── offlineQueue.ts
│       │   └── App.tsx
│       └── package.json
│
└── tests/
    ├── RentFlow.Domain.Tests/
    ├── RentFlow.Application.Tests/
    ├── RentFlow.Infrastructure.Tests/
    └── RentFlow.WebAPI.IntegrationTests/
```

---

## 3. Domain Model ve İlişkiler

### 3.1 Ana Varlıklar (Entities)

#### **Tenant** (Kiracı)
```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public TenantStatus Status { get; set; } // Demo, Active, Suspended
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpgradedAt { get; set; }
    
    // Limits (enforced at app level)
    public int MaxVehicles { get; set; }
    public int MaxUsers { get; set; }
    public int CurrentVehicleCount { get; set; }
    public int CurrentUserCount { get; set; }
    
    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
```

**İş Kuralları:**
- Demo tenant oluşturulduğunda `Status=Demo`, `MaxVehicles=3`, `MaxUsers=2`.
- Ödeme başarılı olduğunda `Status=Active`, limitler plan'a göre güncellenir.
- Suspended tenant kullanıcıları login olamaz (middleware katmanında kontrol).

---

#### **Vehicle** (Araç)
```csharp
public class Vehicle
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // Multi-tenancy discriminator
    public string LicensePlate { get; set; } = null!; // PK alternatifi: Unique(TenantId, LicensePlate)
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public VehicleStatus Status { get; set; } // Available, Rented, Maintenance
    public int CurrentKilometers { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public enum VehicleStatus { Available, Rented, Maintenance }
```

**İş Kuralları:**
- `Status=Maintenance` iken yeni rezervasyon oluşturulamaz.
- Plaka unique olmalı (tenant bazında): `HasIndex(v => new { v.TenantId, v.LicensePlate }).IsUnique()`

---

#### **Booking** (Rezervasyon)
```csharp
public class Booking
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public BookingStatus Status { get; set; } // Pending, Confirmed, CheckedOut, Completed, Cancelled
    public decimal TotalPrice { get; set; }
    
    // Navigation
    public Vehicle Vehicle { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Delivery? Delivery { get; set; } // 1-to-1 ilişki
}

public enum BookingStatus 
{ 
    Pending,      // Yeni oluşturuldu
    Confirmed,    // Onaylandı
    CheckedOut,   // Araç teslim edildi
    Completed,    // İade yapıldı
    Cancelled     // İptal edildi
}
```

**Kritik İş Kuralları (Çakışma Önleme):**
1. **Veritabanı Constraint:**
   ```sql
   CREATE UNIQUE INDEX IX_Booking_NoOverlap 
   ON Bookings(VehicleId, StartDate, EndDate)
   WHERE Status NOT IN ('Cancelled');
   ```
   (Not: SQL Server `PERIOD` constraint kullanılabilir veya trigger ile enforced olabilir.)

2. **Application-Level Check (Optimistic):**
   ```csharp
   public async Task<bool> HasConflictAsync(Guid vehicleId, DateTime start, DateTime end, Guid? excludeBookingId = null)
   {
       return await _context.Bookings
           .Where(b => b.VehicleId == vehicleId 
                    && b.Status != BookingStatus.Cancelled
                    && b.Id != excludeBookingId
                    && b.StartDate < end 
                    && b.EndDate > start)
           .AnyAsync();
   }
   ```
   Kullanıcıya mesaj: **"Bu araç seçilen tarihlerde dolu."**

3. **Concurrency Handling:**
   - `Booking` entity'sine `RowVersion` (byte[]) ekle.
   - EF Core `IsRowVersion()` ile otomatik optimistic concurrency.
   - Çakışma durumunda `DbUpdateConcurrencyException` → retry logic veya kullanıcıya "Takvim değişti, lütfen yenileyin."

---

#### **Delivery** (Teslim/İade)
```csharp
public class Delivery
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; } // 1-to-1 with Booking
    public Guid TenantId { get; set; }
    public Guid AssignedUserId { get; set; } // Saha personeli
    
    // Check-Out (Teslim)
    public DateTime? CheckOutAt { get; set; }
    public int? CheckOutKm { get; set; }
    public string? CheckOutFuelLevel { get; set; } // "Full", "3/4", "Half", "1/4", "Empty"
    public Guid? CheckOutReportId { get; set; }
    
    // Check-In (İade)
    public DateTime? CheckInAt { get; set; }
    public int? CheckInKm { get; set; }
    public string? CheckInFuelLevel { get; set; }
    public Guid? CheckInReportId { get; set; }
    
    // Navigation
    public Booking Booking { get; set; } = null!;
    public User AssignedUser { get; set; } = null!;
    public DeliveryReport? CheckOutReport { get; set; }
    public DeliveryReport? CheckInReport { get; set; }
    public ICollection<DeliveryPhoto> Photos { get; set; } = new List<DeliveryPhoto>();
}
```

---

#### **DeliveryReport** (PDF Tutanak)
```csharp
public class DeliveryReport
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public ReportType Type { get; set; } // CheckOut, CheckIn
    public string PdfBlobUrl { get; set; } = null!; // Azure Blob Storage URL
    public DateTime GeneratedAt { get; set; }
    public string CustomerSignatureBase64 { get; set; } = null!;
    
    // Navigation
    public Delivery Delivery { get; set; } = null!;
}

public enum ReportType { CheckOut, CheckIn }
```

**Kabul Kriteri:**
- PDF, mobil uygulamadan "Tutanak Oluştur" butonuna basıldıktan sonra **5 saniye içinde** oluşturulup `PdfBlobUrl`'e yazılmalıdır.
- Signature canvas'tan alınan imza Base64 olarak kaydedilir ve PDF'e embeds edilir.

---

#### **DeliveryPhoto** (Fotoğraf)
```csharp
public class DeliveryPhoto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public PhotoAngle Angle { get; set; } // Front, Back, Left, Right
    public PhotoType Type { get; set; } // CheckOut, CheckIn
    public string BlobUrl { get; set; } = null!;
    public DateTime CapturedAt { get; set; }
    
    // Navigation
    public Delivery Delivery { get; set; } = null!;
}

public enum PhotoAngle { Front, Back, Left, Right }
public enum PhotoType { CheckOut, CheckIn }
```

**Kabul Kriteri:**
- Mobil uygulama, teslim/iade sırasında **4 zorunlu açıdan** (Front, Back, Left, Right) fotoğraf çekilmeden "Tutanak Oluştur" butonunu aktif etmemelidir.
- Fotoğraflar JPEG formatında, 1080p max çözünürlükte, Azure Blob'a yüklenir.

---

#### **Customer** (Müşteri)
```csharp
public class Customer
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? IdentityNumber { get; set; } // TC Kimlik No
    public string? DrivingLicenseNumber { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
```

**Validation (FluentValidation):**
- `Phone`: Türkiye formatı regex (`^(05)[0-9]{9}$`)
- `Email`: RFC 5322 compliant
- `IdentityNumber`: Türkiye TC Kimlik No algoritması (11 basamak, mod 10 checksum)

---

#### **Plan** (Abonelik Planı)
```csharp
public class Plan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!; // "Demo", "Temel", "Profesyonel", "Kurumsal"
    public decimal MonthlyPrice { get; set; }
    public int MaxVehicles { get; set; }
    public int MaxUsers { get; set; }
    public bool IsDemo { get; set; }
    
    // Navigation
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
```

**Seed Data (Migration):**
```csharp
modelBuilder.Entity<Plan>().HasData(
    new Plan { Id = Guid.Parse("..."), Name = "Demo", MonthlyPrice = 0, MaxVehicles = 3, MaxUsers = 2, IsDemo = true },
    new Plan { Id = Guid.Parse("..."), Name = "Temel", MonthlyPrice = 1490, MaxVehicles = 10, MaxUsers = 3, IsDemo = false },
    new Plan { Id = Guid.Parse("..."), Name = "Profesyonel", MonthlyPrice = 2990, MaxVehicles = 30, MaxUsers = 8, IsDemo = false },
    new Plan { Id = Guid.Parse("..."), Name = "Kurumsal", MonthlyPrice = 5990, MaxVehicles = int.MaxValue, MaxUsers = 15, IsDemo = false }
);
```

---

#### **User** (Kullanıcı)
```csharp
public class User : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public UserRole Role { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; } = null!;
}

public enum UserRole 
{ 
    SaaSAdmin,       // Platform yöneticisi (TenantId=null veya özel sentinel value)
    TenantAdmin,     // Ofis yöneticisi
    FrontDesk,       // Ön masa personeli
    FieldStaff       // Saha personeli (mobil)
}
```

**Authorization Policy:**
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTenantAdmin", policy => policy.RequireRole("TenantAdmin"));
    options.AddPolicy("RequireFrontDesk", policy => policy.RequireRole("TenantAdmin", "FrontDesk"));
    options.AddPolicy("RequireFieldStaff", policy => policy.RequireRole("FieldStaff"));
    options.AddPolicy("RequireSaaSAdmin", policy => policy.RequireRole("SaaSAdmin"));
});
```

---

### 3.2 Value Objects

#### **DateRange**
```csharp
public record DateRange(DateTime Start, DateTime End)
{
    public bool Overlaps(DateRange other) 
        => Start < other.End && End > other.Start;
    
    public int DurationInDays 
        => (End.Date - Start.Date).Days;
}
```

Kullanım:
```csharp
var bookingRange = new DateRange(booking.StartDate, booking.EndDate);
if (bookingRange.Overlaps(existingRange)) 
    throw new BookingConflictException();
```

---

## 4. Kritik Kullanım Senaryoları (Use Cases)

### 4.1 Self-Signup ve Demo Tenant Oluşturma

**Akış:**
1. Kullanıcı `POST /api/auth/signup` endpoint'ine `{ CompanyName, Email, Password }` gönderir.
2. `CreateDemoTenantCommandHandler`:
   - `Plan.IsDemo=true` olan planı bulur.
   - Yeni `Tenant` oluşturur (`Status=Demo`, limitler planınkiyle set edilir).
   - `User` oluşturur (`Role=TenantAdmin`, `TenantId` atanır).
   - ASP.NET Identity ile kullanıcı kaydeder.
   - Hoş geldin e-postası gönderir (background job: Hangfire/Azure Functions).
3. Response: `{ TenantId, UserId, RedirectUrl: "/onboarding" }`

**Kabul Kriteri:**
- Toplam süre **<5 dakika** (manuel, UI üzerinden).
- Onboarding wizard: "İlk aracınızı ekleyin" → "İlk müşterinizi ekleyin" → "İlk rezervasyonunuzu oluşturun."

---

### 4.2 Rezervasyon Oluşturma (Çakışma Kontrolü ile)

**Akış:**
1. Ön masa personeli `POST /api/bookings` ile yeni rezervasyon oluşturur.
2. `CreateBookingCommandHandler`:
   ```csharp
   // 1. Validation
   var vehicle = await _vehicleRepo.GetByIdAsync(command.VehicleId);
   if (vehicle == null || vehicle.Status != VehicleStatus.Available)
       throw new ValidationException("Araç kullanılamaz.");
   
   // 2. Conflict check
   var hasConflict = await _bookingRepo.HasConflictAsync(
       command.VehicleId, command.StartDate, command.EndDate);
   if (hasConflict)
       throw new BookingConflictException("Bu araç seçilen tarihlerde dolu.");
   
   // 3. Demo limit check
   if (_currentTenant.Status == TenantStatus.Demo 
       && _currentTenant.CurrentBookingCount >= 5)
       throw new DemoLimitExceededException("Demo limitinize ulaştınız.");
   
   // 4. Create booking
   var booking = new Booking 
   { 
       TenantId = _currentTenantId,
       VehicleId = command.VehicleId,
       CustomerId = command.CustomerId,
       StartDate = command.StartDate,
       EndDate = command.EndDate,
       Status = BookingStatus.Confirmed,
       TotalPrice = CalculatePrice(vehicle, command.StartDate, command.EndDate)
   };
   
   await _bookingRepo.AddAsync(booking);
   await _unitOfWork.CommitAsync();
   ```

**Hata Yönetimi:**
- `DbUpdateException` (constraint violation) → "Başka bir kullanıcı aynı aracı rezerve etti. Lütfen takvimi yenileyin."
- `DbUpdateConcurrencyException` → Retry logic (Polly) veya kullanıcıya bilgi.

**Performans Kabul Kriteri:**
- 100 araç + 500 rezervasyon verisiyle rezervasyon takvimi **<1.5 saniye** yüklenmelidir.
- Sorgu optimizasyonu:
  ```csharp
  var bookings = await _context.Bookings
      .Include(b => b.Vehicle)
      .Include(b => b.Customer)
      .Where(b => b.StartDate >= startOfMonth && b.EndDate <= endOfMonth)
      .AsNoTracking()
      .ToListAsync();
  ```

---

### 4.3 Mobil Teslim (Check-Out) Akışı

**Akış (Mobil App → Backend API):**
1. Saha personeli `GET /api/deliveries/assigned` ile kendisine atanan teslimatları listeler.
2. Teslimat seçilir → `CheckOutScreen.tsx` açılır.
3. **Fotoğraf Çekme:**
   - `PhotoCapture.tsx` component'i 4 açı için (`Front`, `Back`, `Left`, `Right`) zorunlu fotoğraf ister.
   - Her fotoğraf çekildiğinde local state'e kaydedilir.
   - 4 fotoğraf tamamlanmadan "İleri" butonu disabled.
4. **Form Doldurma:**
   - Kilometre (number input, min=mevcut km)
   - Yakıt seviyesi (dropdown: Full, 3/4, Half, 1/4, Empty)
5. **İmza Alma:**
   - `SignaturePad.tsx` canvas'ı açılır.
   - Müşteri imza atar → Base64'e convert edilir.
6. **Gönderme:**
   ```typescript
   POST /api/deliveries/{id}/checkout
   Content-Type: multipart/form-data
   
   Body:
   - km: 12500
   - fuelLevel: "Full"
   - signatureBase64: "data:image/png;base64,iVBORw0KG..."
   - photos[0]: (binary, angle=Front)
   - photos[1]: (binary, angle=Back)
   - photos[2]: (binary, angle=Left)
   - photos[3]: (binary, angle=Right)
   ```

**Backend Handler (`CompleteCheckOutCommandHandler`):**
```csharp
public async Task<CheckOutResult> Handle(CompleteCheckOutCommand request, CancellationToken ct)
{
    var delivery = await _deliveryRepo.GetByIdAsync(request.DeliveryId);
    
    // 1. Upload photos to Blob
    var photoUrls = new List<string>();
    foreach (var photo in request.Photos)
    {
        var url = await _blobService.UploadAsync(
            container: $"tenant-{_currentTenantId}",
            fileName: $"delivery-{delivery.Id}-{photo.Angle}-{DateTime.UtcNow:yyyyMMddHHmmss}.jpg",
            stream: photo.Stream
        );
        photoUrls.Add(url);
        
        delivery.Photos.Add(new DeliveryPhoto 
        { 
            Angle = photo.Angle, 
            Type = PhotoType.CheckOut, 
            BlobUrl = url 
        });
    }
    
    // 2. Update delivery
    delivery.CheckOutAt = DateTime.UtcNow;
    delivery.CheckOutKm = request.Km;
    delivery.CheckOutFuelLevel = request.FuelLevel;
    
    // 3. Generate PDF
    var pdfUrl = await _pdfService.GenerateCheckOutReportAsync(new PdfRequest
    {
        DeliveryId = delivery.Id,
        CustomerName = delivery.Booking.Customer.FullName,
        VehiclePlate = delivery.Booking.Vehicle.LicensePlate,
        CheckOutKm = request.Km,
        PhotoUrls = photoUrls,
        SignatureBase64 = request.SignatureBase64
    });
    
    var report = new DeliveryReport
    {
        DeliveryId = delivery.Id,
        Type = ReportType.CheckOut,
        PdfBlobUrl = pdfUrl,
        CustomerSignatureBase64 = request.SignatureBase64,
        GeneratedAt = DateTime.UtcNow
    };
    
    delivery.CheckOutReport = report;
    delivery.Booking.Status = BookingStatus.CheckedOut;
    
    await _unitOfWork.CommitAsync();
    
    return new CheckOutResult { PdfUrl = pdfUrl };
}
```

**Offline Support (Mobil):**
- Eğer `POST /checkout` başarısız olursa (network hatası):
  ```typescript
  // offlineQueue.ts
  await AsyncStorage.setItem(`pending-checkout-${deliveryId}`, JSON.stringify({
      deliveryId,
      km,
      fuelLevel,
      signatureBase64,
      photoUris: [uri1, uri2, uri3, uri4],
      timestamp: Date.now()
  }));
  ```
- Background sync (Expo TaskManager):
  - Her 5 dakikada veya ağ tekrar geldiğinde kuyruğu kontrol et.
  - Başarılı olursa local storage'dan sil.

**Kabul Kriteri:**
- PDF oluşturma süresi **<5 saniye**.
- 4 fotoğraf + imza zorunlu; eksik olduğunda buton disable.

---

### 4.4 Demodan Ücretli Plana Geçiş (Iyzico Entegrasyonu)

**Akış:**
1. Kullanıcı demo limitine ulaştığında in-app banner gösterilir: "Demo limitinize ulaştınız. Yükseltmek için tıklayın."
2. `/plans` sayfasında plan karşılaştırma tablosu gösterilir.
3. Kullanıcı "Temel" planı seçer → `POST /api/payments/initiate`:
   ```csharp
   public async Task<IyzicoCheckoutResult> InitiatePayment(Guid planId)
   {
       var plan = await _planRepo.GetByIdAsync(planId);
       var tenant = await _tenantRepo.GetByIdAsync(_currentTenantId);
       
       var request = new CreateCheckoutFormInitializeRequest
       {
           Price = plan.MonthlyPrice.ToString("F2", CultureInfo.InvariantCulture),
           PaidPrice = plan.MonthlyPrice.ToString("F2", CultureInfo.InvariantCulture),
           Currency = Currency.TRY.ToString(),
           BasketId = Guid.NewGuid().ToString(),
           PaymentGroup = PaymentGroup.SUBSCRIPTION.ToString(),
           CallbackUrl = $"{_config.BaseUrl}/payments/callback",
           Buyer = new Buyer { ... },
           BasketItems = new List<BasketItem> 
           { 
               new() { Name = plan.Name, Price = ... } 
           }
       };
       
       var response = CheckoutFormInitialize.Create(request, _iyzicoOptions);
       
       if (response.Status == "success")
       {
           // Store token for later verification
           await _cache.SetAsync($"payment-token-{response.Token}", new PaymentSession 
           { 
               TenantId = tenant.Id, 
               PlanId = plan.Id, 
               Token = response.Token 
           });
           
           return new IyzicoCheckoutResult 
           { 
               CheckoutFormContent = response.CheckoutFormContent, 
               Token = response.Token 
           };
       }
       
       throw new PaymentInitiationException(response.ErrorMessage);
   }
   ```

4. Frontend, `CheckoutFormContent` (HTML snippet) içinde iFrame render eder.
5. Kullanıcı Iyzico UI'ında ödeme yapar → Iyzico, `CallbackUrl`'e POST yapar.
6. `POST /payments/callback` handler:
   ```csharp
   public async Task<IActionResult> PaymentCallback([FromForm] string token)
   {
       var result = CheckoutForm.Retrieve(new RetrieveCheckoutFormRequest { Token = token }, _iyzicoOptions);
       
       if (result.PaymentStatus == "SUCCESS")
       {
           var session = await _cache.GetAsync<PaymentSession>($"payment-token-{token}");
           var tenant = await _tenantRepo.GetByIdAsync(session.TenantId);
           var plan = await _planRepo.GetByIdAsync(session.PlanId);
           
           tenant.PlanId = plan.Id;
           tenant.Status = TenantStatus.Active;
           tenant.MaxVehicles = plan.MaxVehicles;
           tenant.MaxUsers = plan.MaxUsers;
           tenant.UpgradedAt = DateTime.UtcNow;
           
           await _unitOfWork.CommitAsync();
           
           // Send confirmation email
           await _emailService.SendAsync(tenant.Email, "Planınız Aktifleşti", ...);
           
           return Redirect("/dashboard?upgraded=true");
       }
       
       return Redirect("/plans?error=payment_failed");
   }
   ```

**Hata Senaryoları:**
- Ödeme başarısız: Kullanıcı `/plans?error=payment_failed` sayfasına yönlendirilir → "Ödeme işlemi tamamlanamadı. Lütfen tekrar deneyin."
- Iyzico timeout: Retry logic (Polly) veya kullanıcıyı manuel kontrol sayfasına yönlendir.

---

## 5. Teknik Standartlar ve Best Practices

### 5.1 Kod Kalitesi
- **C# Standartları:**
  - `dotnet format` zorunlu (CI/CD pipeline'da enforce).
  - Nullable reference types enabled (`<Nullable>enable</Nullable>`).
  - Primary constructors (C# 12) ve record types kullan.
  - Pattern matching: `if (vehicle is { Status: VehicleStatus.Available })`.

- **SOLID Prensipleri:**
  - **SRP:** Her handler bir use case.
  - **OCP:** Strategy pattern (örn. `IPdfGenerator` interface → `QuestPdfGenerator`, `ITextSharpGenerator`).
  - **LSP:** Base repository abstract class kullanımında türev sınıflar Liskov'u ihlal etmemeli.
  - **ISP:** `IBookingRepository` gibi spesifik interface'ler; `IGenericRepository` kullanma.
  - **DIP:** Tüm bağımlılıklar interface üzerinden inject edilmeli.

### 5.2 Test Stratejisi (TDD)
- **Unit Tests:**
  - Domain logic (örn. `DateRange.Overlaps()`).
  - Validators (FluentValidation test'leri).
  - Handlers (mock repository'ler ile).
  
- **Integration Tests:**
  - WebAPI endpoints: `WebApplicationFactory<Program>` ile in-memory test server.
  - EF Core: Test veritabanı (SQLite in-memory veya Testcontainers ile SQL Server).
  
  Örnek:
  ```csharp
  [Fact]
  public async Task CreateBooking_WhenConflictExists_ShouldReturn409()
  {
      // Arrange
      var client = _factory.CreateClient();
      await SeedExistingBooking(vehicleId: _testVehicleId, start: "2025-01-10", end: "2025-01-15");
      
      var command = new { VehicleId = _testVehicleId, StartDate = "2025-01-12", EndDate = "2025-01-20" };
      
      // Act
      var response = await client.PostAsJsonAsync("/api/bookings", command);
      
      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
      var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
      problem.Title.Should().Be("Booking conflict");
  }
  ```

### 5.3 Performans Optimizasyonu
- **EF Core:**
  - `AsNoTracking()` read-only sorgularda.
  - Compiled queries hot path'lerde:
    ```csharp
    private static readonly Func<RentFlowDbContext, Guid, DateTime, DateTime, Task<bool>> _conflictQuery =
        EF.CompileAsyncQuery((RentFlowDbContext ctx, Guid vehicleId, DateTime start, DateTime end) =>
            ctx.Bookings.Any(b => b.VehicleId == vehicleId && b.StartDate < end && b.EndDate > start));
    ```
  - N+1 önleme: `Include()` ve `ThenInclude()` kullan, GraphQL varsa DataLoader.
  
- **Caching:**
  - Distributed cache (Redis): Plan listesi, sık erişilen tenant metadata.
  - Response caching: Takvim API'si (`Cache-Control: max-age=60`).
  
- **BenchmarkDotNet:**
  - Critical path'ler için (örn. `BookingConflictChecker.HasConflictAsync`) benchmark yaz.
  ```csharp
  [Benchmark]
  public async Task<bool> CheckConflict_100Bookings()
  {
      return await _checker.HasConflictAsync(_vehicleId, _start, _end);
  }
  ```

### 5.4 Güvenlik (OWASP Top 10)
- **Injection Prevention:**
  - Parameterized queries (EF Core otomatik yapar).
  - Input validation: FluentValidation tüm command'lerde.
  
- **Authentication:**
  - JWT tokens (mobil API): `exp` claim + refresh token rotation.
  - Cookie-based (Blazor): `HttpOnly`, `Secure`, `SameSite=Strict`.
  
- **Authorization:**
  - Tenant isolation: Global query filter + resource-based authorization.
  - Example:
    ```csharp
    var authResult = await _authService.AuthorizeAsync(User, booking, "CanEditBooking");
    if (!authResult.Succeeded) return Forbid();
    ```

- **Secrets Management:**
  - Azure Key Vault: Iyzico API key, connection strings.
  - `dotnet user-secrets` (development).
  
- **Rate Limiting:**
  - ASP.NET Core 7+ built-in: `AddRateLimiter()`.
  - Policy: 100 requests/min per user, 1000 requests/min per tenant.

### 5.5 Observability
- **Structured Logging (Serilog):**
  ```csharp
  Log.Information("Booking created: {BookingId} for Vehicle {VehicleId} by User {UserId}", 
      booking.Id, booking.VehicleId, _currentUserId);
  ```
  
- **Distributed Tracing (OpenTelemetry):**
  - Trace ID propagation: API → Database → Blob Storage.
  - Export to Application Insights.
  
- **Health Checks:**
  ```csharp
  builder.Services.AddHealthChecks()
      .AddDbContextCheck<RentFlowDbContext>()
      .AddAzureBlobStorage(_config.BlobConnectionString)
      .AddUrlGroup(new Uri(_config.IyzicoApiUrl), "Iyzico");
  ```
  Endpoint: `/health` (UI: AspNetCore.HealthChecks.UI).

- **Metrics (Prometheus/App Insights):**
  - Custom metrics: `bookings_created_total`, `pdf_generation_duration_seconds`.

---

## 6. Mobil Uygulama (React Native - Expo)

### 6.1 Temel Yapı
```
RentFlow.Mobile/
├── src/
│   ├── screens/
│   │   ├── LoginScreen.tsx
│   │   ├── DeliveryListScreen.tsx
│   │   ├── CheckOutScreen.tsx
│   │   └── CheckInScreen.tsx
│   ├── components/
│   │   ├── PhotoCapture.tsx       # 4 açı fotoğraf çekme wizard
│   │   ├── SignaturePad.tsx       # react-native-signature-canvas
│   │   └── OfflineIndicator.tsx
│   ├── services/
│   │   ├── apiClient.ts           # Axios instance (JWT auth)
│   │   ├── offlineQueue.ts        # AsyncStorage-based queue
│   │   └── syncService.ts         # Background sync (Expo TaskManager)
│   ├── navigation/
│   │   └── AppNavigator.tsx       # React Navigation stack
│   └── store/
│       ├── authSlice.ts           # Redux Toolkit
│       └── deliverySlice.ts
├── app.json
└── package.json
```

### 6.2 Kritik Özellikler

#### **Offline Fotoğraf ve Senkronizasyon**
```typescript
// offlineQueue.ts
export const queueCheckOut = async (payload: CheckOutPayload) => {
  const queue = await AsyncStorage.getItem(QUEUE_KEY) || '[]';
  const items = JSON.parse(queue);
  items.push({ id: uuid(), type: 'CHECK_OUT', payload, timestamp: Date.now() });
  await AsyncStorage.setItem(QUEUE_KEY, JSON.stringify(items));
};

export const processQueue = async () => {
  const queue = await AsyncStorage.getItem(QUEUE_KEY) || '[]';
  const items: QueueItem[] = JSON.parse(queue);
  
  for (const item of items) {
    try {
      if (item.type === 'CHECK_OUT') {
        await apiClient.post(`/deliveries/${item.payload.deliveryId}/checkout`, item.payload);
        // Remove from queue
        const remaining = items.filter(i => i.id !== item.id);
        await AsyncStorage.setItem(QUEUE_KEY, JSON.stringify(remaining));
      }
    } catch (error) {
      console.error('Sync failed:', error);
      // Retry later
    }
  }
};

// syncService.ts (Expo TaskManager)
TaskManager.defineTask(BACKGROUND_SYNC_TASK, async () => {
  const netInfo = await NetInfo.fetch();
  if (netInfo.isConnected) {
    await processQueue();
  }
  return BackgroundFetch.BackgroundFetchResult.NewData;
});
```

#### **Fotoğraf Yakalama**
```typescript
// PhotoCapture.tsx
const PhotoCapture: React.FC<{ onComplete: (photos: Photo[]) => void }> = ({ onComplete }) => {
  const angles: PhotoAngle[] = ['Front', 'Back', 'Left', 'Right'];
  const [currentIndex, setCurrentIndex] = useState(0);
  const [photos, setPhotos] = useState<Photo[]>([]);
  
  const handleCapture = async () => {
    const result = await ImagePicker.launchCameraAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      quality: 0.8, // Balance between quality and size
      exif: false,
    });
    
    if (!result.canceled) {
      const newPhotos = [...photos, { angle: angles[currentIndex], uri: result.assets[0].uri }];
      setPhotos(newPhotos);
      
      if (currentIndex === angles.length - 1) {
        onComplete(newPhotos);
      } else {
        setCurrentIndex(currentIndex + 1);
      }
    }
  };
  
  return (
    <View>
      <Text>Fotoğraf {currentIndex + 1}/4: {angles[currentIndex]} Açı</Text>
      <Button title="Fotoğraf Çek" onPress={handleCapture} />
    </View>
  );
};
```

---

## 7. Deployment ve DevOps

### 7.1 CI/CD Pipeline (GitHub Actions)
```yaml
# .github/workflows/backend-ci.yml
name: Backend CI/CD

on:
  push:
    branches: [main, develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run tests
        run: dotnet test --no-build --configuration Release --logger "trx;LogFileName=test-results.trx"
      
      - name: Code coverage
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Publish
        run: dotnet publish src/RentFlow.WebAPI/RentFlow.WebAPI.csproj -c Release -o ./publish
      
      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v2
        with:
          app-name: rentflow-api-prod
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

### 7.2 Azure Infrastructure (IaC - Terraform/Bicep)
```hcl
# Örnek Terraform snippet
resource "azurerm_app_service" "rentflow_api" {
  name                = "rentflow-api-prod"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.ai.instrumentation_key
    "ConnectionStrings__DefaultConnection" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.db_connection.id})"
  }
}

resource "azurerm_sql_database" "rentflow_db" {
  name                = "rentflow-prod"
  server_name         = azurerm_sql_server.sql.name
  resource_group_name = azurerm_resource_group.rg.name
  sku_name            = "S1"
}
```

---

## 8. Sık Sorulan Teknik Sorular (FAQ)

**S: Aynı anda iki kullanıcı aynı araç için rezervasyon oluşturursa ne olur?**  
C: 
1. Application-level: `HasConflictAsync()` her ikisi için de `false` döner (race condition).
2. Database-level: Unique constraint veya trigger devreye girer, ikinci `INSERT` başarısız olur.
3. EF Core `DbUpdateException` fırlatır → middleware yakalayıp `409 Conflict` döner.
4. Frontend: "Başka bir kullanıcı bu aracı rezerve etti. Lütfen takvimi yenileyin."

**S: Demo limitleri nasıl enforce ediliyor?**  
C: Her entity ekleme işlemi öncesi handler içinde:
```csharp
if (_currentTenant.Status == TenantStatus.Demo)
{
    var currentCount = await _context.Vehicles.CountAsync();
    if (currentCount >= _currentTenant.MaxVehicles)
        throw new DemoLimitExceededException("Demo araç limitinize ulaştınız.");
}
```

**S: Mobil uygulama offline iken fotoğraflar nasıl saklanıyor?**  
C: 
- Fotoğraflar `expo-file-system` ile local'e kaydedilir.
- Metadata + URI, AsyncStorage'da queue'ya eklenir.
- Background task (Expo TaskManager) her 5 dakikada veya ağ geldiğinde queue'yu işler.
- Başarılı upload sonrası local dosya silinir.

**S: TenantId claim'i nasıl set ediliyor?**  
C: Login sırasında:
```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new("TenantId", user.TenantId.ToString()),
    new(ClaimTypes.Role, user.Role.ToString())
};
```
Middleware'de:
```csharp
var tenantIdClaim = httpContext.User.FindFirst("TenantId")?.Value;
if (Guid.TryParse(tenantIdClaim, out var tenantId))
    _currentTenantId = tenantId;
```

---

## 9. Performans Hedefleri ve SLA'lar

| Metrik | Hedef | Ölçüm Yöntemi |
|--------|-------|---------------|
| Rezervasyon takvimi yükleme (100 araç, 500 rezervasyon) | <1.5s | Application Insights, p95 |
| PDF tutanak oluşturma | <5s | Custom metric: `pdf_generation_duration` |
| API response time (p95) | <500ms | Application Insights |
| Self-signup → demo panele erişim | <5 dakika | Analytics event tracking |
| Mobil fotoğraf upload (4 fotoğraf, 2MB total) | <10s | Mobile app telemetry |
| Database query time (booking conflict check) | <100ms | EF Core query logging + App Insights |

---

## 10. Güvenlik Kontrol Listesi

- [ ] **Input Validation:** Tüm command'lerde FluentValidation.
- [ ] **SQL Injection:** EF Core parameterized queries (otomatik).
- [ ] **XSS:** Blazor otomatik encode eder; raw HTML kullanımında `MarkupString` dikkatli kullan.
- [ ] **CSRF:** Blazor Server otomatik anti-forgery token ekler.
- [ ] **Tenant Isolation:** Global query filter + authorization handler test edildi mi?
- [ ] **Secrets:** Tüm secret'lar Key Vault'ta mı? Kodda hardcoded secret yok mu?
- [ ] **HTTPS Enforcement:** `app.UseHttpsRedirection()` + HSTS.
- [ ] **Rate Limiting:** API endpoint'lerinde aktif mi?
- [ ] **Logging:** PII (Personally Identifiable Information) loglanmıyor mu? (örn. TC Kimlik No, kredi kartı).

---

## 11. Copilot Chat Kullanım Önerileri

Bu dosyayı VS 2022'de açık tuttuğunuzda, Copilot Chat'e şu şekillerde sorular sorabilirsiniz:

**Örnek Sorular:**
- "RentFlow'da yeni bir rezervasyon oluştururken hangi validasyonlar yapılıyor?"
- "Mobil uygulamada offline fotoğraf upload nasıl çalışıyor? Kod örneği ver."
- "BookingConflictChecker sınıfı için unit test nasıl yazmalıyım?"
- "Demo tenant limitlerini aşan bir kullanıcıya ne tür bir hata döndürülmeli?"
- "TenantId global query filter'ı EF Core'da nasıl yapılandırılmış? Kodunu göster."
- "Iyzico ödeme callback'inde hangi adımlar takip ediliyor?"
- "DeliveryReport PDF'i nasıl oluşturuluyor? Performans optimizasyonu için ne yapmalıyım?"

**Copilot'a Bağlam Verme:**
```
@workspace Bu dosyayı referans alarak, RentFlow projesinde çakışma önleme mantığı için 
integration test yaz. 100 araç ve 500 rezervasyon senaryosunu kapsamalı.
```

---

## 12. Changelog (Context Dosyası)

| Versiyon | Tarih | Değişiklikler |
|----------|-------|---------------|
| 1.0 | 2025-01-23 | İlk oluşturma. PRD 3.6 tam mapping. |

---

**Son Not:** Bu dosya, PRD'nin teknik bir yansımasıdır. İş kuralları değiştiğinde veya yeni özellikler eklendiğinde **mutlaka güncellenmelidir**. Copilot'un doğru öneriler sunması için her zaman sync tutulmalıdır.