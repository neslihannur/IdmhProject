namespace IdmhProject.Services;

public record PhotoCredit(string File, string Author, string License, string Source);

/// Sitedeki fotoğrafların kaynakları (Wikimedia Commons, serbest lisanslar).
public static class Credits
{
    public static readonly IReadOnlyList<PhotoCredit> All =
    [
        new("brick-house", "Tom Mrazek", "CC BY 3.0", "https://commons.wikimedia.org/wiki/File:Modern_Architecture_(119443449).jpeg"),
        new("facades", "Picturepest", "CC BY 2.0", "https://commons.wikimedia.org/wiki/File:Facade_-_Flickr_-_Picturepest.jpg"),
        new("glass-office", "Peter Balcerzak", "CC BY-SA 3.0", "https://commons.wikimedia.org/wiki/File:Glass_Facade_(211945331).jpeg"),
        new("glass-office-2", "Dietmar Rabich", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:M%C3%BCnster,_LVM-Versicherung_--_2017_--_6848_--_bw.jpg"),
        new("court", "Dietmar Rabich", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:M%C3%BCnster,_Westdeutsche_Lotterie_--_2017_--_6852_--_bw.jpg"),
        new("louvers", "Dietmar Rabich", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:D%C3%BClmen,_Kirchspiel,_Marienkapelle_Visbeck,_Orgel_--_2022_--_4143_(bw).jpg"),
        new("facade-vertical", "Osman Rana osmanrana", "CC0", "https://commons.wikimedia.org/wiki/File:Black_and_white_facade_(Unsplash).jpg"),
        new("concrete-steps", "Jules Verne Times Two", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:Concrete_seats_at_Portas_do_Mar,_Ponta_Delgada,_S%C3%A3o_Miguel_Island,_Azores,_Azores,_Portugal_(PPL1-Corrected).jpg"),
        new("housing", "lafleur", "CC BY 2.0", "https://commons.wikimedia.org/wiki/File:Soviet_apartment.jpg"),
        new("white-wall", "Tyto Albatraoz", "CC BY 3.0", "https://commons.wikimedia.org/wiki/File:White_Concrete_(217280287).jpeg"),
        new("white-house", "Jules Verne Times Two", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:House_with_shadows_cast_onto_staircase,_Myrsini,_Tinos,_Greece_julesvernex2-2.jpg"),
        new("spiral-lamp", "Scott Barnard", "CC BY 3.0", "https://commons.wikimedia.org/wiki/File:Light_Bulb_Staircase_(107995289).jpeg"),
        new("stair-shadow", "Jules Verne Times Two", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:Staircase_of_the_Church_of_Saints_William_and_Peregrine,_Foggia,_Italy_(PPL2-Enhanced)_julesvernex2.jpg"),
        new("stone-stair", "Jules Verne Times Two", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:Stone_staircase,_Santu%C3%A1rio_da_Peninha,_Sintra,_Portugal_julesvernex2-2.jpg"),
        new("columns", "Marco Dias", "CC BY 3.0", "https://commons.wikimedia.org/wiki/File:Perspective_To_The_Top_(238520339).jpeg"),
        new("lounge", "w_lemay", "CC BY-SA 2.0", "https://commons.wikimedia.org/wiki/File:Interior,_Duncan_House,_Polymath_Park,_Acme,_PA.jpg"),
        new("dining", "Shixart1985", "CC BY 2.0", "https://commons.wikimedia.org/wiki/File:Modern_dining_area_with_stylish_table_and_chairs_in_cozy_interior_design.jpg"),
        new("kitchen", "Jeremy Levine from Los Angeles, United States", "CC BY 2.0", "https://commons.wikimedia.org/wiki/File:Sustainable_Kitchen_-_Flickr_-_Jeremy_Levine_Design.jpg"),
        new("campus-night", "Dietmar Rabich", "CC BY-SA 4.0", "https://commons.wikimedia.org/wiki/File:M%C3%BCnster,_WestLotto_--_2013_--_3296.jpg"),
        new("hotel-room", "Shixart1985", "CC BY 2.0", "https://commons.wikimedia.org/wiki/File:Modern_bedroom_design_in_a_stylish_hotel_room_featuring_geometric_patterns_and_soft_linens.jpg"),
        new("plan", "HABS / Library of Congress", "Public domain", "https://commons.wikimedia.org/wiki/File:First_Floor_Plan_-_Ellis_Island,_New_Immigration_Building,_New_York_Harbor,_New_York,_New_York_County,_NY_HABS_NY-6086-O_(sheet_2_of_6).png"),
        new("cafe", "Mack Male from Edmonton, AB, Canada", "CC BY-SA 2.0", "https://commons.wikimedia.org/wiki/File:Jelly_Modern_Doughnuts_(5826788764).jpg"),
        new("sketch", "Bilinmeyen sanatçı (Cooper Hewitt)", "Public domain", "https://commons.wikimedia.org/wiki/File:Drawing,_Desk_for_Mrs._Allen,_Henry_J._Allen_Residence,_Wichita,_Kansas,_1917_(CH_18800291).jpg"),
    ];
}
