SLUTRAPPORT CsharpSnackis (Publicerat: Motor-duon Forum) 
Mattias Hermansson Salmi mattias.hermansson.salmi@iths.se
Antonio Santana antonio.santana@iths.se 
Trello board https://trello.com/b/Nega4dSe/csharpsnackis
Gannt-chart finns på Trello
1.	Beskrivning av projektet
Projektet är ett forum för diskussioner om valfria ämnen. Som grund finns stöd för just fordon. 
Avvikelser from urspungsplanen: Projektet saknar en tydlig API-gateway, istället sker alla REST-anrop från sidornas code-behind (cshtml.cs). I övrigt saknas avvikelser.
2.	Tidsöversikt
Start för projektet var 2021-05-18. Slutdatum var 2021-06-15. Uppskattad tidsåtgång är 100 timmar. 
Inget tidsbortfall har inträffat under perioden. 
En felbedömning avseende mängden tid för att ordna relationer i SQL-databasen och ta bort relationer i ett led. Detta påverkade dock inte projektet negativt.
Se bifogat Gannt-diagram.
3.	Tekniska krav
Teknisk plattform: Windows
Serverlösning: IIS
Ramverk: .NET CORE 5
Bibliotek/Nuget-paket: API-projektet (CSharpsnackisDB.sln):
Microsoft.AspNetCore: Diagnostics.EntityFrameworkCore, Identity.EntityFrameworkCore, .Identity.UI, .Authentication.JwtBearer, .SqlServer, EntityFrameworkCore, Microsoft: .EntityFrameWorkCore.SqlServer, .EntityFrameworkCore.Tools, .Extensions.DependencyInjection, .VisualStudio.Web.CodeGeneration.Design .NewtonSoft.Json SwashBucke.AspNetCore
Webb-projektet (CSharpSnackisApp.sln)
Microsoft.AspNetCore.Http NewtonSoft.Json System.Windows.Extension
Datalösning: MS SQL
Servermiljö: Azure
4.	Applikationsstruktur.
Projekt: CsharpSnackisApp (Site)
Beroenden: SnackisApi.cs (Bas-adress genereras för REST-anrop), PostReactionModel.cs, CategoryResponseModel.cs, User.cs (Modell-klasser), SessionCheck.cs (Konvertera token från aktuell session till byte-array). Startup.cs konfigureras cookies, session och möjligheten att via @inject använda IHttpContextAccessor, för att komma åt lagrad information i aktuell session.
Programmeringsmönster: ASP.NET CORE, Razor Pages.
Projekt: CharpSnackisDB (API/DB) 
Beroenden: Random (Generera en slumpad tråd från exponerad API-metod). I Startup.cs sker konfiguering av autentisering och JwT-bearer tokens.
API-Struktur: Se SwaggerUI för controllerinformation: https://csharpsnackisdb20210614110849.azurewebsites.net/swagger/index.html 
Micro Services: API:et som enhet utgör Micro service.
Programmeringsmönster: Webb-API.
5.	Installation.
Projektet som helhet är publicerat på Azure: https://motorduonforum.azurewebsites.net/ 
https://csharpsnackisdb20210614110849.azurewebsites.net/swagger/index.html 
För installation använd Visual Studios publish funtionalitet. Connection-strings finns i App-settings.json filen. API-projektet: Aktuell migration är genomförd vid överlämnandet. Vid debugging på lokal SQL-server krävs update-database i package manager console. Fördefinerade roller finns. Önskas detta ändras sker detta i Context.cs. Seedning av innehåll, vänligen se SeedController.cs.

6.	Endpoints.
Vänligen se SwaggerUI för en beskrivning av Endpoints. Endpoints har namngivits för att vara självförklarande. Vid frågor vänligen kontakta utvecklarna.

7.	Funktionalitet.
Meny:
Samtliga besökare: Länk till exponerad API-metod för en slumpmässig tråd i forumet. Cookies, användaren godkänner användandet av cookies. Info om GDPR finns.
Icke inloggad: Registrering/Inloggning, Microsoft Identity server som har anpassats med fler egenskaper används. Auktorisering sker med stöd av JwT-bearer token som lagras i en session hos användaren. Vid registrering kan uppladdning ske av visningsbild.
Inloggad användare: Mina sidor, Användaren kan ändra profilbild eller radera den, detsamma gäller all information. Bilder sparas på webb-appens server. Bilder tas alltid bort från servern vid önskad radering. Andra användare som besöker profilen kan rapportera en olämplig användare.
Chatt: möjlighet att vid markering av flera namn (ctrl + click) skapa gruppchatt. Vid val av en mottagare blir det en enkel chatt, dessa syns under ”aktiva chattar”. Uppdatering av vald chatt kan ske vid behov för kontroll om nytt meddelande mottagits.
Inloggad Administratör: Mina sidor, Samma funktionalitet som hos en användare. En admin kan ändra information och radera bilder vid besök på en användares profil.
Admin dashboard, administratör kan se statistik över antal användare, poster i forumet, antal trådar och rapporterade objekt (poster, svar och användare). Enkelt att besöka rapporterade profiler och inlägg. Inlägg som rapporterats markeras upp och kan enkelt identifieras. Det går att bannlysa användare. När granskning genomförts av objekt kan detta tas bort i menyn.
Forumet:
Icke inloggad användare kan endast läsa inlägg och navigera på forumet mellan kategorier, ämnen och trådar. Inloggning krävs för att använda annan funktionalitet. 
Inloggade användare kan utöver detta skapa trådar, poster och svar. Reaktioner kan lämnas på inlägg och foton kan laddas upp i alla led. Radering är möjlig i alla led (endast om användaren är upphovsmakare) Rapportering av olämpliga inlägg kan ske.

Inloggad administratör kan utöver detta skapa nya kategorier och ämnen. Ändring och radering är möjlig. Administratör är behörig att ta bort och ändra inlägg av användare.
Ett filter finns för diverse fula ord som ersätter den del av ordet som fastnat i filtret med asterisker. 
8.	Brister/ 9. Förbättringsförslag
Ordfiltrering (API-metod): Filtret använder Regex och stöd saknas för annat än bokstäver [a-z, A-Z]. Vid annan inmatning kommer filtret att släppa igenom allt.
Optimering av antalet anrop till API:et kan ses över, för att undvika onödig belastning på databasen.
Möjlighet att radera chattar saknas, detta bör implementeras. Idag behöver vi använda SQL-frågor på Azure för att ta bort dessa.
Återsändning av formulär kan vid skickande av chattar och vid reaktioner på inlägg optimeras. En lösning är att även här implementera omdirigering och lagra nödvändig data i aktuell session.

10	.  Summering
Arbetet har gått extremt bra. Vi har haft en bra arbetsdynamik och metodik.
De utmaningarna som har uppstått var vid publicering av projektet då server är i en annan tidszon (UTC) samt användandet av statiska fält vid flera inloggade användare (bytte till session istället).
De lärdomarna som uppstått har varit hur man arbetar i en mindre grupp och par programmering än innan.
Absolut vi trivs bra som kollegor samt vänner. Detta har resulterat till att vi har LIA på samma arbetsplats.

