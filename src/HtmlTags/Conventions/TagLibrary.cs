using System;
using System.Linq;

namespace HtmlTags.Conventions
{
    public interface ITagLibrary
    {
        ITagPlan PlanFor(ElementRequest subject, string profile = null, string category = null);
    }

    public class TagLibrary : ITagBuildingExpression, ITagLibrary, IVisitable
    {
        private readonly Cache<string, TagCategory> _categories =
            new Cache<string, TagCategory>(name => new TagCategory());

        public ITagPlan PlanFor(ElementRequest subject, string profile = null, string category = null)
        {
            profile = profile ?? TagConstants.Default;
            category = category ?? TagConstants.Default;

            return _categories[category].PlanFor(subject, profile);
        }

        public CategoryExpression Always
        {
            get { return _categories[TagConstants.Default].Always; }
        }

        public CategoryExpression If(Func<ElementRequest, bool> matches)
        {
            return _categories[TagConstants.Default].If(matches);
        }

        public void Add(Func<ElementRequest, bool> filter, ITagBuilder builder)
        {
            Add(new ConditionalTagBuilderPolicy(filter, builder));
        }

        /// <summary>
        /// The modifiers and builders for a category of conventions 
        /// </summary>
        /// <param name="category">Example:  "Label", "Editor", "Display"</param>
        /// <returns></returns>
        public TagCategory Category(string category)
        {
            return _categories[category];
        }

        public BuilderSet BuilderSetFor(string category = null, string profile = null)
        {
            profile = profile ?? TagConstants.Default;
            category = category ?? TagConstants.Default;

            return _categories[category].Profile(profile);
        }

        /// <summary>
        /// Adds a builder policy to the default category and profile
        /// </summary>
        /// <param name="policy"> </param>
        public void Add(ITagBuilderPolicy policy)
        {
            Default.Add(policy);
        }

        /// <summary>
        /// Adds a modifier to the default category and profile
        /// </summary>
        /// <param name="modifier"></param>
        public void Add(ITagModifier modifier)
        {
            Default.Add(modifier);
        }

        /// <summary>
        /// Access to the default category
        /// </summary>
        public TagCategory Default
        {
            get
            {
                return _categories[TagConstants.Default];
            }
        }

        /// <summary>
        /// Add builders and modifiers by profile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public ITagBuildingExpression ForProfile(string profile)
        {
            return _categories[TagConstants.Default].ForProfile(profile);
        }

        public void Import(TagLibrary other)
        {
            var keys = _categories.GetKeys().Union(other._categories.GetKeys()).Distinct();

            keys.Each(key => _categories[key].Import(other._categories[key]));
        }

        // virtual for mocking
        public virtual void AcceptVisitor(ITagLibraryVisitor visitor)
        {
            _categories.Each((name, category) => {
                visitor.Category(name, category);
                category.AcceptVisitor(visitor);
            });
        }

        public void AcceptVisitor(IHtmlConventionVisitor visitor)
        {
            AcceptVisitor(visitor.VisitorFor());
        }
    }
}