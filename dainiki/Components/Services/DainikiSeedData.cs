namespace dainiki.Components.Services;

using dainiki.Components.Models;
using Microsoft.EntityFrameworkCore;

public static class DainikiSeedData
{
    public static async Task SeedAsync(DainikiDbContext context)
    {
        await EnsureCategoriesAsync(context);
        await EnsureMoodsAsync(context);
        await EnsureTagsAsync(context);
    }

    private static async Task EnsureCategoriesAsync(DainikiDbContext context)
    {
        var moodCategories = new[] { "Positive", "Neutral", "Negative" };
        var journalCategories = new[] { "Personal", "Work", "Health", "Travel", "Reflection" };

        foreach (var name in moodCategories)
        {
            if (!await context.Categories.AnyAsync(category =>
                    category.category_name == name && category.category_type == "Mood"))
            {
                context.Categories.Add(new Category
                {
                    category_name = name,
                    category_type = "Mood"
                });
            }
        }

        foreach (var name in journalCategories)
        {
            if (!await context.Categories.AnyAsync(category =>
                    category.category_name == name && category.category_type == "Journal"))
            {
                context.Categories.Add(new Category
                {
                    category_name = name,
                    category_type = "Journal"
                });
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureMoodsAsync(DainikiDbContext context)
    {
        if (await context.Moods.AnyAsync())
        {
            return;
        }

        var moodCategories = await context.Categories
            .Where(category => category.category_type == "Mood")
            .ToDictionaryAsync(category => category.category_name, category => category.category_id);

        var moods = new List<Mood>
        {
            new Mood { mood_name = "Happy", category_id = moodCategories["Positive"] },
            new Mood { mood_name = "Excited", category_id = moodCategories["Positive"] },
            new Mood { mood_name = "Relaxed", category_id = moodCategories["Positive"] },
            new Mood { mood_name = "Grateful", category_id = moodCategories["Positive"] },
            new Mood { mood_name = "Confident", category_id = moodCategories["Positive"] },
            new Mood { mood_name = "Calm", category_id = moodCategories["Neutral"] },
            new Mood { mood_name = "Thoughtful", category_id = moodCategories["Neutral"] },
            new Mood { mood_name = "Curious", category_id = moodCategories["Neutral"] },
            new Mood { mood_name = "Nostalgic", category_id = moodCategories["Neutral"] },
            new Mood { mood_name = "Bored", category_id = moodCategories["Neutral"] },
            new Mood { mood_name = "Sad", category_id = moodCategories["Negative"] },
            new Mood { mood_name = "Angry", category_id = moodCategories["Negative"] },
            new Mood { mood_name = "Stressed", category_id = moodCategories["Negative"] },
            new Mood { mood_name = "Lonely", category_id = moodCategories["Negative"] },
            new Mood { mood_name = "Anxious", category_id = moodCategories["Negative"] }
        };

        context.Moods.AddRange(moods);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureTagsAsync(DainikiDbContext context)
    {
        if (await context.Tags.AnyAsync())
        {
            return;
        }

        var tags = new[]
        {
            "Work", "Career", "Studies", "Family", "Friends", "Relationships",
            "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel", "Nature",
            "Finance", "Spirituality", "Birthday", "Holiday", "Vacation", "Celebration", "Exercise",
            "Reading", "Writing", "Cooking", "Meditation", "Yoga", "Music", "Shopping", "Parenting",
            "Projects", "Planning", "Reflection"
        };

        context.Tags.AddRange(tags.Select(tag => new Tag
        {
            tag_name = tag,
            system_tag = true
        }));

        await context.SaveChangesAsync();
    }
}
