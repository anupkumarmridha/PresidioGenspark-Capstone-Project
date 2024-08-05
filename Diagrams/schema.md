## Schema Overview

### Entities
1. **User**
2. **NewsArticle**
3. **Comment**
4. **Reaction**

### Relationships
- A `User` can have many `Comments`.
- A `User` can have many `Reactions`.
- A `NewsArticle` can have many `Comments`.
- A `NewsArticle` can have many `Reactions`.
- A `Comment` can have many replies (self-referencing).
- A `Comment` belongs to a `NewsArticle` and a `User`.
- A `Reaction` belongs to a `NewsArticle` and a `User`.

## Schema Details

### User
- **Id** (int): Primary Key
- **Email** (string): Unique email address
- **DisplayName** (string): User's display name
- **Role** (string): Role of the user (e.g., Admin, User)
- **GoogleId** (string): Google OAuth ID
- **GivenName** (string): User's first name
- **FamilyName** (string): User's last name
- **Picture** (string): URL to the user's profile picture
- **CreatedAt** (DateTime): Date and time of account creation
- **UpdatedAt** (DateTime): Date and time of last account update

### NewsArticle
- **Id** (string): Primary Key, unique identifier for the article
- **Author** (string): Author of the article
- **Content** (string): Content of the article
- **Date** (DateTime): Publication date of the article
- **Title** (string): Title of the article
- **ImageUrl** (string): URL to an image associated with the article
- **ReadMoreUrl** (string): URL to the full article
- **Status** (string): Status of the article (e.g., pending, approved, rejected)
- **Category** (string): Category of the article
- **Comments** (ICollection<Comment>): Navigation property for related comments
- **Reactions** (ICollection<Reaction>): Navigation property for related reactions

### Comment
- **Id** (int): Primary Key
- **ArticleId** (string): Foreign Key, references `NewsArticle`
- **Content** (string): The content of the comment
- **CreatedAt** (DateTime): Date and time the comment was created
- **UserId** (int): Foreign Key, references `User`
- **User** (User): Navigation property for the user who made the comment
- **ParentId** (int?): Nullable Foreign Key, references `Comment` for replies
- **Parent** (Comment): Navigation property for the parent comment
- **Replies** (ICollection<Comment>): Navigation property for replies
- **NewsArticle** (NewsArticle): Navigation property for the related article

### Reaction
- **Id** (int): Primary Key
- **ArticleId** (string): Foreign Key, references `NewsArticle`
- **UserId** (int): Foreign Key, references `User`
- **ReactionType** (ReactionType): Enum, indicates if the reaction is a Like or Dislike
- **User** (User): Navigation property for the user who made the reaction
- **NewsArticle** (NewsArticle): Navigation property for the related article

## Relationships

1. **User and Comment**
   - One-to-Many: A `User` can post multiple `Comments`.
   - A `Comment` is associated with a single `User`.

2. **User and Reaction**
   - One-to-Many: A `User` can have multiple `Reactions`.
   - A `Reaction` is associated with a single `User`.

3. **NewsArticle and Comment**
   - One-to-Many: A `NewsArticle` can have multiple `Comments`.
   - A `Comment` is associated with a single `NewsArticle`.

4. **NewsArticle and Reaction**
   - One-to-Many: A `NewsArticle` can have multiple `Reactions`.
   - A `Reaction` is associated with a single `NewsArticle`.

5. **Comment and Comment (Replies)**
   - One-to-Many (Self-referencing): A `Comment` can have multiple replies, and each reply can itself be a `Comment`.

6. **Comment and NewsArticle**
   - Many-to-One: A `Comment` is associated with a single `NewsArticle`.

7. **Reaction and NewsArticle**
   - Many-to-One: A `Reaction` is associated with a single `NewsArticle`.
