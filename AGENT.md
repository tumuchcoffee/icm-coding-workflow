# AI Brand Tone

## Writing Style

- **Professional but approachable** - Avoid overly formal or academic language
- **Technically precise** - Use correct terminology and be accurate
- **Clear and concise** - Prefer short, direct sentences over long explanations
- **Active voice preferred** - "Configure the API" not "The API should be configured"
- **Write for developers** - Assume technical competence, don't oversimplify
- **Avoid condescension** - Never use "simply", "just", "obviously", or "clearly"

## Terminology

### Preferred Terms

- Use "repository" not "repo" in documentation
- Use "pull request" not "PR" in formal writing
- Use "authentication" not "auth" in user-facing content
- Use "database" not "DB" in documentation
- Use "function" not "method" unless specifically referring to class methods
- Use "configure" not "setup" (setup is a noun, set up is a verb)

### Words to Avoid

- **Jargon**: "synergy", "leverage", "utilize", "paradigm", "ecosystem" (unless technically accurate)
- **Condescending**: "simply", "just", "obviously", "clearly", "of course"
- **Vague**: "stuff", "things", "a bunch of"
- **Filler**: "basically", "actually", "really", "very"
- **Passive constructions**: "It is recommended that..." → "We recommend..."

### Product Names & Capitalization

- ProductName (example - replace with your actual product names)
- API (all caps)
- JavaScript (capital J, capital S)
- TypeScript (capital T, capital S)
- GitHub (capital G, capital H)
- PostgreSQL (not Postgres in formal docs)

## Code Comments

- **Be concise** - Comments should explain "why", not "what"
- **Use complete sentences** - Start with capital letter, end with period
- **Avoid obvious comments** - Don't comment `// Increment counter` for `counter++`
- **Document intent** - Explain business logic, edge cases, and non-obvious decisions

### Good Comment Examples

```javascript
// Retry failed requests up to 3 times to handle transient network errors.
const maxRetries = 3;

// Cache results for 5 minutes to reduce database load during peak hours.
const cacheTimeout = 300000;
```

### Avoid

```javascript
// Set max retries to 3
const maxRetries = 3;

// This is the cache timeout
const cacheTimeout = 300000;
```

## Documentation

- **Start with the "why"** - Explain the purpose before the implementation
- **Include examples** - Show real-world usage, not just API signatures
- **Link to related docs** - Help users discover connected concepts
- **Keep it up to date** - Update docs when code changes
- **Use consistent formatting** - Follow markdown best practices

## Examples

### Good ✅

"Configure authentication by adding your API key to the environment variables. The application reads `API_KEY` on startup and uses it for all external requests."

"This function validates user input before processing. We check for SQL injection patterns because user data flows directly into database queries."

### Avoid ❌

"Simply leverage the auth config to synergize your API credentials and you'll be good to go!"

"This function does validation stuff. Obviously you need to check things before using them."

---

## Customization Instructions

**Replace the bracketed sections and examples above with your actual:**
- Product names and capitalization rules
- Specific terminology preferences
- Real examples from your codebase
- Any additional style guidelines specific to your team

**Keep this file concise** - This is loaded into every prompt, so focus on the most important guidelines that have the biggest impact on consistency.

## What Not To Do

- If you don't know something, don't assume. Ask me for clarification.
- Don't make up information or make assumptions about the codebase.
- Always ask for approval when running commands or making changes.