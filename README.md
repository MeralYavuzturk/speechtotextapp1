# Speech to Text
# 🎤 Vosk Tabanlı Mikrofon Ses Tanıma Uygulaması (C# & NAudio)
### Çalıştırılabilir zip dosyası linki:
## Bu proje, C# Windows Forms uygulaması kullanarak mikrofon üzerinden alınan sesi gerçek zamanlı olarak tanımak ve metne dönüştürmek için açık kaynaklı **Vosk** motorunu ve **NAudio** kütüphanesini kullanır.
## ✨ Özellikler
* **Gerçek Zamanlı Ses Tanıma:** Mikrofon girişini anlık olarak işler.
* **Çoklu Dil Desteği (Türkçe & Almanca):** Dil seçeneği üzerinden farklı Vosk modelleri arasında geçiş yapabilme yeteneği.
* **NAudio Entegrasyonu:** Güvenilir ve düşük gecikmeli mikrofon girişi yönetimi.
* **Durum Bildirimleri:** Uygulamanın anlık durumunu (Dinliyor, Durduruldu, Hata) gösterir.
### 4. Uygulamayı Çalıştırma
1.  Visual Studio'da projeyi derleyin ve çalıştırın.
2.  Uygulama açıldığında, **Vosk sistemi otomatik olarak varsayılan dil (Türkçe) ile başlatılacaktır.**
3.  Arayüzdeki **Başlat** butonuna tıklayarak mikrofon dinlemeyi başlatın.
## 🖱️ Kullanım

| Kontrol Öğesi | İşlevi |
| :--- | :--- |
| **Dil Seçimi (ComboBox)** | Kullanılacak Vosk modelini seçer (Türkçe / Almanca). Yeni bir dil seçimi, Vosk sistemini yeniden yükler. |
| **Başlat Butonu** | Mikrofon dinlemesini başlatır ve sesi tanımaya başlar. |
| **Durdur Butonu** | Dinlemeyi durdurur ve aradaki son konuşmayı işleyerek ekrana yazar. |
| **Temizle Butonu** | Sonuç metin kutusunu temizler. |
| **Durum Etiketi** | Uygulamanın anlık durumunu gösterir (hazır, dinliyor, hata vb.). |
