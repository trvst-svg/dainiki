namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;

    public static class DainikiSeedData
    {
        public static async Task Seed(DainikiDbContext context)
        {
            await EnsureCategories(context);
            await EnsureMoods(context);
            await EnsureTags(context);
        }

        private static async Task EnsureCategories(DainikiDbContext context)
        {
            string[] moodCategories = { "Positive", "Neutral", "Negative" };
            string[] journalCategories = { "Personal", "Work", "Health", "Travel", "Reflection" };

            foreach (string name in moodCategories)
            {
                bool exists = await context.Categories.AnyAsync(category =>
                    category.category_name == name && category.category_type == "Mood");

                if (!exists)
                {
                    Category category = new Category
                    {
                        category_name = name,
                        category_type = "Mood"
                    };
                    context.Categories.Add(category);
                }
            }

            foreach (string name in journalCategories)
            {
                bool exists = await context.Categories.AnyAsync(category =>
                    category.category_name == name && category.category_type == "Journal");

                if (!exists)
                {
                    Category category = new Category
                    {
                        category_name = name,
                        category_type = "Journal"
                    };
                    context.Categories.Add(category);
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureMoods(DainikiDbContext context)
        {
            bool hasMoods = await context.Moods.AnyAsync();
            if (hasMoods)
            {
                return;
            }

            List<Category> moodCategories = await context.Categories
                .Where(category => category.category_type == "Mood")
                .ToListAsync();

            Dictionary<string, int> categoryIds = new Dictionary<string, int>();
            foreach (Category category in moodCategories)
            {
                categoryIds[category.category_name] = category.category_id;
            }

            List<Mood> moods = new List<Mood>
            {
                new Mood { mood_name = "Happy", category_id = categoryIds["Positive"] },
                new Mood { mood_name = "Excited", category_id = categoryIds["Positive"] },
                new Mood { mood_name = "Relaxed", category_id = categoryIds["Positive"] },
                new Mood { mood_name = "Grateful", category_id = categoryIds["Positive"] },
                new Mood { mood_name = "Confident", category_id = categoryIds["Positive"] },
                new Mood { mood_name = "Calm", category_id = categoryIds["Neutral"] },
                new Mood { mood_name = "Thoughtful", category_id = categoryIds["Neutral"] },
                new Mood { mood_name = "Curious", category_id = categoryIds["Neutral"] },
                new Mood { mood_name = "Nostalgic", category_id = categoryIds["Neutral"] },
                new Mood { mood_name = "Bored", category_id = categoryIds["Neutral"] },
                new Mood { mood_name = "Sad", category_id = categoryIds["Negative"] },
                new Mood { mood_name = "Angry", category_id = categoryIds["Negative"] },
                new Mood { mood_name = "Stressed", category_id = categoryIds["Negative"] },
                new Mood { mood_name = "Lonely", category_id = categoryIds["Negative"] },
                new Mood { mood_name = "Anxious", category_id = categoryIds["Negative"] }
            };

            context.Moods.AddRange(moods);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureTags(DainikiDbContext context)
        {
            bool hasTags = await context.Tags.AnyAsync();
            if (hasTags)
            {
                return;
            }

            string[] tags =
            {
                "Work", "Career", "Studies", "Family", "Friends", "Relationships",
                "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel", "Nature",
                "Finance", "Spirituality", "Birthday", "Holiday", "Vacation", "Celebration", "Exercise",
                "Reading", "Writing", "Cooking", "Meditation", "Yoga", "Music", "Shopping", "Parenting",
                "Projects", "Planning", "Reflection"
            };

            List<Tag> tagEntities = new List<Tag>();
            foreach (string tag in tags)
            {
                Tag tagEntity = new Tag
                {
                    tag_name = tag,
                    system_tag = true
                };
                tagEntities.Add(tagEntity);
            }

            context.Tags.AddRange(tagEntities);
            await context.SaveChangesAsync();
        }
    }
}
