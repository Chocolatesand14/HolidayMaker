# Holiday Maker – Tankar och frågor inför onsdagens handledning

## Det som har gjorts

Kommunikationen mellan C# och MySQL fungerar nu med MySqlConnector och `async/await`.

Koden har delats upp i `UI` och `Repositories` för att göra den mer lättläst och underhållbar. Tanken är att hålla användargränssnittet och databashanteringen åtskilda.

Hittills fungerar kundregistrering, sökning efter lediga rum med olika filter och att skapa bokningar. Även kontroller mot dubbelbokningar och för många gäster har testats och verkar fungera som tänkt.

## Funderingar och frågor

**Kodstruktur**

- Finns fortfarande en SQL-fråga i `ConsoleMenu.cs`. Borde även den flyttas till ett repository?
- Skulle det vara bättre att ha en separat mapp för modeller, eller blir det onödigt för ett så här litet projekt?

**Bokningshantering**

- Använder en transaktion tillsammans med `FOR UPDATE` för att kontrollera tillgängligheten innan bokningen sparas. Är det en rimlig lösning, eller finns det ett enklare sätt med tanke på uppgiftens omfattning?
- Finns det något mer att tänka på när bokningar ska ändras eller tas bort?

**Säkerhet**

- SQL-frågorna är parametriserade och databaslösenordet ligger utanför källkoden. Finns det fler säkerhetsaspekter som vore bra att tänka på när C# kommunicerar med MySQL?

**Databasstruktur**

- Det finns en `payment`-tabell som inte används i applikationen just nu. Är det bättre att behålla den eller förenkla databasen?

## Det som återstår

Nästa steg är att kunna visa kundernas bokningar, ändra antal gäster och avboka. Därefter återstår att testa hela bokningsflödet och färdigställa dokumentationen.

Vill framför allt få feedback på kodstrukturen och bokningshanteringen innan projektet färdigställs.