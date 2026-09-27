using Friendout.Domain.Context;
using Friendout.Domain.Models;
using Bogus;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Seeds;

public static class ActivitySeeder
{
    /// <summary>
    /// One entry per activity "genre": sample titles/descriptions to pick from,
    /// a pool of relevant sub-activity names (steps/moments of the day) and a
    /// pool of relevant equipment names.
    /// </summary>
    private sealed record ActivityTemplate(
        string[] Titles,
        string[] Descriptions,
        string[] SubActivityNames,
        string[] Equipments);

    private static readonly ActivityTemplate[] Templates =
    [
        new ActivityTemplate(
            Titles: ["Randonnée au sommet du Puy de Dôme", "Balade en forêt de Fontainebleau", "Trek dans les Calanques de Marseille", "Randonnée nocturne sous les étoiles"],
            Descriptions:
            [
                "Une randonnée de quelques heures à travers des paysages magnifiques, prévoir de bonnes chaussures et de l'eau.",
                "Balade tranquille en pleine nature, accessible à tous niveaux, parfaite pour discuter entre amis.",
                "Sortie sportive avec un bon dénivelé, pique-nique prévu au sommet."
            ],
            SubActivityNames: ["Montée", "Pause pique-nique", "Point de vue", "Descente"],
            Equipments: ["Chaussures de randonnée", "Sac à dos", "Gourde", "Bâtons de marche", "Coupe-vent", "Lampe frontale"]),

        new ActivityTemplate(
            Titles: ["Week-end camping au bord du lac", "Bivouac en montagne", "Camping sauvage en forêt"],
            Descriptions:
            [
                "Deux jours de camping avec feu de camp, jeux et baignade si la météo le permet.",
                "Une nuit sous les étoiles loin de tout, déconnexion garantie.",
                "Week-end détente en pleine nature avec activités pour tous les goûts."
            ],
            SubActivityNames: ["Installation du camp", "Feu de camp", "Randonnée matinale", "Rangement du site"],
            Equipments: ["Tente", "Sac de couchage", "Matelas gonflable", "Lampe frontale", "Réchaud de camping", "Glacière"]),

        new ActivityTemplate(
            Titles: ["Match de foot entre amis", "Tournoi de volley sur la plage", "Session de basket au parc"],
            Descriptions:
            [
                "Match amical, tous niveaux bienvenus, ambiance détendue garantie.",
                "Petit tournoi convivial, pensez à venir avec une tenue de sport.",
                "Session sportive suivie d'un moment convivial pour récupérer ensemble."
            ],
            SubActivityNames: ["Échauffement", "Match", "Temps de récupération"],
            Equipments: ["Ballon", "Maillots", "Chasubles", "Sifflet", "Filet"]),

        new ActivityTemplate(
            Titles: ["Soirée jeux de société", "Tournoi de jeux vidéo", "Escape game entre amis"],
            Descriptions:
            [
                "Une soirée conviviale autour de plusieurs jeux, boissons et grignotage prévus.",
                "Tournoi amical avec quelques lots à la clé, ambiance bonne humeur assurée.",
                "Session d'énigmes en équipe, réservation faite à l'avance."
            ],
            SubActivityNames: ["Accueil", "Premières parties", "Pause goûter", "Finale"],
            Equipments: ["Jeux de société", "Cartes", "Dés", "Manettes"]),

        new ActivityTemplate(
            Titles: ["Barbecue entre amis", "Repas de crémaillère", "Brunch du dimanche"],
            Descriptions:
            [
                "Repas convivial en extérieur, chacun ramène un plat ou une boisson.",
                "On se retrouve autour d'un bon repas fait maison pour fêter ça ensemble.",
                "Brunch tranquille pour bien démarrer le dimanche."
            ],
            SubActivityNames: ["Préparatifs", "Apéritif", "Repas", "Digestif"],
            Equipments: ["Barbecue", "Charbon", "Glacière", "Ustensiles de cuisine", "Vaisselle réutilisable"]),

        new ActivityTemplate(
            Titles: ["Sortie kayak sur la rivière", "Session paddle au lac", "Après-midi voile"],
            Descriptions:
            [
                "Descente de rivière tranquille, encadrement possible pour les débutants.",
                "Initiation ou perfectionnement selon le niveau de chacun, matériel fourni sur place.",
                "Sortie nautique avec pique-nique sur une plage accessible uniquement par l'eau."
            ],
            SubActivityNames: ["Briefing sécurité", "Mise à l'eau", "Navigation", "Retour au ponton"],
            Equipments: ["Combinaison néoprène", "Kayak", "Gilet de sauvetage", "Palmes", "Sac étanche"]),

        new ActivityTemplate(
            Titles: ["Session d'escalade en salle", "Grimpe en falaise", "Bloc entre amis"],
            Descriptions:
            [
                "Session grimpe pour tous niveaux, initiation possible sur place.",
                "Sortie en extérieur, encadrement par un grimpeur expérimenté du groupe.",
                "Ambiance décontractée autour du bloc, parfait pour progresser en groupe."
            ],
            SubActivityNames: ["Échauffement", "Voies faciles", "Voies difficiles", "Étirements"],
            Equipments: ["Baudrier", "Corde d'escalade", "Chaussons d'escalade", "Casque", "Magnésie"]),

        new ActivityTemplate(
            Titles: ["Sortie vélo dans la campagne", "Balade VTT en forêt", "Rando vélo urbaine"],
            Descriptions:
            [
                "Parcours tranquille adapté à tous, pause photo prévue en chemin.",
                "Sortie plus sportive avec quelques passages techniques.",
                "Balade urbaine à la découverte de nouveaux quartiers à vélo."
            ],
            SubActivityNames: ["Départ", "Pause ravitaillement", "Arrivée"],
            Equipments: ["Vélo", "Casque", "Kit de réparation", "Antivol", "Gourde"]),

        new ActivityTemplate(
            Titles: ["Journée ski entre amis", "Week-end snowboard", "Initiation raquettes en montagne"],
            Descriptions:
            [
                "Journée sur les pistes, tous niveaux bienvenus, pause au chalet en milieu de journée.",
                "Week-end détente à la montagne entre glisse et raclette.",
                "Balade en raquettes accessible aux débutants, encadrement possible."
            ],
            SubActivityNames: ["Matinée sur les pistes", "Pause déjeuner", "Après-midi sur les pistes", "Après-ski"],
            Equipments: ["Skis", "Chaussures de ski", "Casque", "Lunettes de ski", "Gants"]),

        new ActivityTemplate(
            Titles: ["Séance cinéma entre amis", "Sortie théâtre", "Concert en plein air"],
            Descriptions:
            [
                "Séance suivie d'un verre pour débriefer le film.",
                "Sortie culturelle, réservation des places à faire à l'avance.",
                "Concert en extérieur, pensez à prévoir une couverture pour s'installer."
            ],
            SubActivityNames: ["Séance", "Débrief autour d'un verre"],
            Equipments: ["Billets", "Couverture de pique-nique"])
    ];

    public static async Task SeedAsync(FriendoutDbContext db)
    {
        int amountActivityToSeed = 20;

        if (db.Activities.Any())
            return;

        var user = db.Users.FirstOrDefault();
        string userId = user?.Id ?? Guid.NewGuid().ToString(); // fallback si aucun user

        var faker = new Faker();
        var equipmentCache = await GetOrCreateEquipmentAsync(db, Templates.SelectMany(t => t.Equipments));

        for (int i = 0; i < amountActivityToSeed; i++)
        {
            var template = faker.PickRandom(Templates);

            var localisation = new Localisation
            {
                Type = LocalisationType.Address,
                Address = faker.Address.FullAddress()
            };
            db.Localisations.Add(localisation);

            var startAt = faker.Date.Future();
            var endAt = startAt.AddHours(faker.Random.Double(2, 8));

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = faker.PickRandom(template.Titles),
                Description = faker.PickRandom(template.Descriptions),
                StartAt = startAt,
                EndAt = endAt,
                Localisation = localisation,
                EstimatedPrice = faker.Random.Bool(0.7f) ? Math.Round(faker.Random.Double(0, 200), 2) : null,
                ImageId = null,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            AddSubActivities(db, faker, activity, template, startAt, endAt);
            AddEquipments(faker, activity, template, equipmentCache);

            db.Activities.Add(activity);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Loads existing equipment by name and creates any missing ones (Equipment.Name is unique),
    /// so the same equipment row is reused/linked across multiple activities instead of duplicated.
    /// </summary>
    private static async Task<Dictionary<string, Equipment>> GetOrCreateEquipmentAsync(
        FriendoutDbContext db, IEnumerable<string> equipmentNames)
    {
        var distinctNames = equipmentNames.Distinct().ToList();
        var existing = db.Equipment
            .Where(e => distinctNames.Contains(e.Name))
            .ToDictionary(e => e.Name);

        foreach (var name in distinctNames)
        {
            if (existing.ContainsKey(name))
                continue;

            var equipment = new Equipment { Id = Guid.NewGuid().ToString(), Name = name };
            db.Equipment.Add(equipment);
            existing[name] = equipment;
        }

        // Flush so newly created equipment gets persisted Ids we can safely
        // reference from ActivityEquipment rows added later in the same run.
        await db.SaveChangesAsync();

        return existing;
    }

    /// <summary>
    /// Adds 1 to 3 sub-activities to <paramref name="activity"/>, each with its own
    /// localisation and a time slot contained within the parent activity's window.
    /// </summary>
    private static void AddSubActivities(
        FriendoutDbContext db, Faker faker, Activity activity, ActivityTemplate template,
        DateTime startAt, DateTime endAt)
    {
        int subActivityCount = faker.Random.Int(1, Math.Min(3, template.SubActivityNames.Length));
        var chosenNames = faker.PickRandom(template.SubActivityNames, subActivityCount).ToList();

        var totalMinutes = Math.Max(1, (endAt - startAt).TotalMinutes);
        var sliceLength = totalMinutes / chosenNames.Count;

        for (int i = 0; i < chosenNames.Count; i++)
        {
            var subStart = startAt.AddMinutes(sliceLength * i);
            var subEnd = subStart.AddMinutes(sliceLength * faker.Random.Double(0.5, 1));

            var subLocalisation = new Localisation
            {
                Type = LocalisationType.Address,
                Address = faker.Address.FullAddress()
            };
            db.Localisations.Add(subLocalisation);

            activity.SubActivities.Add(new SubActivity
            {
                Id = Guid.NewGuid().ToString(),
                Name = chosenNames[i],
                Localisation = subLocalisation,
                StartTime = subStart,
                EndTime = subEnd,
                Description = faker.Lorem.Sentence(),
                Price = faker.Random.Bool(0.3f) ? Math.Round(faker.Random.Double(0, 50), 2) : null,
                ActivityId = activity.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Links 2 to 5 equipment items relevant to the activity's category via ActivityEquipment.
    /// </summary>
    private static void AddEquipments(
        Faker faker, Activity activity, ActivityTemplate template,
        Dictionary<string, Equipment> equipmentCache)
    {
        int equipmentCount = faker.Random.Int(2, Math.Min(5, template.Equipments.Length));
        var chosenNames = faker.PickRandom(template.Equipments, equipmentCount).ToList();

        foreach (var name in chosenNames)
        {
            var equipment = equipmentCache[name];

            activity.ActivityEquipments!.Add(new ActivityEquipment
            {
                Id = Guid.NewGuid().ToString(),
                ActivityId = activity.Id,
                EquipmentId = equipment.Id,
                Equipment = equipment,
                Required = faker.Random.Bool(0.6f),
                Quantity = faker.Random.Int(1, 4),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}
