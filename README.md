# DM_Project
Dosad napravljeno :
- MongoDB baza podataka :
    - Modeli : Movie, MovieCollection, User, FacebookFriend, MovieCollectionInfo
    - Kriptografija lozinke korisnika
    - Spremanje filma u bazu podataka 
    - Provjeravanja da li postoji film s proslijeđenim ImdbId-jem
    - Dohvaćanje korisnika po parametru id ili email
    - Registracija i autentifikacija ("običnog")korisnika
    - Registracija facebook korisnika
    - Osvježavanje "običnog" i facebook korisnika
    - Brisanje korisnika preko parametra id
    - Dodavanje filma u kolekciju korisniku uz njegovu ocjenu i komentar
    - Brisanje pojednog filma iz kolekcije proslijeđujući MovieCollectionId
    - Osvježavanje kolekcije pojedinog korisnika
    - Dohvaćanje kolekcije pojedinog korisnika ili dohvaćanje kolekcije facebook prijatelja pojedinog korisnika s podacima o filmovima
    
- Web API (.NET Core 2.2) :
    - Modeli : FacebookApiResponses, LoginUserModel, RegisterUserModel, SearchMovieModel, AppSettings
    - Dohvaćanje podataka sa izvora :
        - www.facebook.com
        - www.themoviedb.org
        - www.omdbapi.com
    - Lijepši prikaz svih mogućih API poziva preko Swagger-a sa mogućnosti autorizacije
    - Autorizacija preko JWT Bearer tokena
    - API ključevi za Imdb, TMDB i Facebook kao i ključ za kriptiranje i FacebookAppID su spremljeni u appsettings.json kao im 
    - Kontroleri :
        - MovieController :
            - Pretraživanje filmova sa TMDB uz parametre : naziv filma, godina i žanr
            - Dohvaćanje svih žanrova preko TMDB API-ja
            - Dohvaćanje popularnih filmova prema TMDB API-ju
            - Dohvaćanje detalja o pojedinom filmu ubacivanjem ID-ja prema TMDB-u i dobivanje detalja sa OMDB API-ja
            - Dohvaćanje preporuke za film preko TMDB API-ja, ali samo na bazi jednog filma
        - UserController :
            - Registriranje "običnog" korisnika
            - Autentifikacija "običnog" korisnika
            - Dodavanje filma u kolekciju korisnika
            - Dohvaćanje kolekcije korisnika
            - Dohvaćanje kolekcije facebook prijatelja od korisnika (preporuka?)
        - ExternalAuthController :
            - Facebook login uz koji ako korisnik ne postoji ga se stvara, ili ako postoji ga se po potrebi osvježava
