using Ae.Galeriya.Core.Tables;

namespace Ae.Galeriya.Core
{
    public static class CategoryExtensions
    {
        public static bool IsTopLevel(this Category category)
        {
            return category.ParentCategoryId == null;
        }

        public static Category FindTopLevel(this Category start)
        {
            var current = start;

            while (!IsTopLevel(current!))
            {
                current = current!.ParentCategory;
            }

            return current!;
        }
    }
}
