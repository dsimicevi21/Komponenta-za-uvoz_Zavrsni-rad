Postavljanje svih potrebnih stvari za rad komponente

- Trebaju .net 8, vs te instalirani postgreSQL i MongoDB
- I postgres i mongodb moraju imati pokrenute windows servise u pozadini, za rad komponente
- kreirati bazu import_demo za postgresql
- Za pripremu baza postoje dvije SQL skripte unutar samples/sql/ za Postgres
- za mongodb se može koristiti ova naredba:

mongosh "mongodb://localhost:27017/import_demo" --eval "
db.students.insertOne({ first_name: 'Petra', last_name: 'Perić', email: 'petra.peric@example.com', year: 1 });
db.knjige.insertOne({ isbn: '978-953-0-30001-1', naziv: 'Na Drini ćuprija', cijena: Double(24.99), dostupna: true, zanr: 'Roman' });
"


- Projekt se pokreće sa multiple startup projects opcijom unutar visual studija, u slučaju da već nije naređen ( solution -> config startup projects -> odabrat prvo api pa ui winforms -> start)
