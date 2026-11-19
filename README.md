# ADS Kill — repository indipendente

ADS Kill è sviluppato qui, **separatamente da AIVEXUS / AI Orchestrator**. Questo repository ospita Android e Windows con compilazioni, release e aggiornamenti autonomi. Budget attuale: zero euro; non sono necessari server AIVEXUS o account utente.

## Prodotti

| Piattaforma | Codice | Stato | Release |
|---|---|---|---|
| Android 10+ | [android/](android/) | **0.1.9 Beta 1**; DNS-only VPN, pulsante arresto, Smart Reward sperimentale, 4 lingue e updater per APK firmati | [Android beta](https://github.com/pilialvu75-hue/adskill/releases/tag/adskill-android-v0.1.9-beta1) |
| Windows 11 x64 | [windows/](windows/) | **0.2.1 Beta 1**; filtro HOSTS, diagnosi DNS Windows (4 domini campione), avviso su emulatori Android, ricerca beta e ZIP verificato | [Windows beta](https://github.com/pilialvu75-hue/adskill/releases/tag/adskill-windows-v0.2.1-beta1) |

**Avvertenza Android:** l'APK debug pubblicato non può sostituire APK debug firmati con certificati differenti. Gli aggiornamenti diretti saranno utilizzabili solo dopo la configurazione una tantum della firma permanente e la prima installazione dell'APK firmato. Il collaudo della disattivazione VPN sul Galaxy S24 FE è ancora aperto. Non disattivare le protezioni del sistema operativo.

## Aggiornamenti autonomi

- Android: repo GitHub di riferimento **`pilialvu75-hue/adskill`**, release `adskill-android-vX.Y.Z-betaN`, asset `AdsKill-Android-vX.Y.Z-betaN-signed.apk`, verifica SHA-256, identità pacchetto, incremento versionCode e certificato. Controllo opzionale all'apertura (una volta al giorno) o manuale; installazione solo con conferma Android.
- Windows: repo GitHub di riferimento **`pilialvu75-hue/adskill`**, release `adskill-windows-vX.Y.Z-betaN`; verifica SHA-256 e sostituzione del ZIP portable solo dopo scelta dell'utente. Windows EXE attualmente non firmato.
- [Istruzioni operative per firma Android permanente](docs/SIGNING_AND_UPDATES.md). Il keystore non viene committato e non viene condiviso con AIVEXUS.
- [Storico dello sviluppo Android originale](docs/ANDROID_HISTORY.md).

## Privacy & Age — lavori sperimentali separati

Sono conservati nei rami `feature/age-privacy-local-preflight` e `feature/privacy-age-edge-browser`, **non integrati** nelle beta di blocco pubblicità. Scopo: aiutare gli adulti a scegliere sistemi di verifica dell'età conformi e a divulgazione minima, senza falsificare prove, aggirare obblighi legali o raccogliere documenti nel nostro server. L'esperimento non deve alterare il comportamento delle app già installate.

Licenze e dipendenze richiedono un audit prima della distribuzione commerciale. Nessuna garanzia di rimozione degli annunci video YouTube: Android usa solo DNS, Windows attualmente filtra HOSTS.
