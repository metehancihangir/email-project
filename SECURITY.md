# Secrets and configuration

Store development credentials with .NET User Secrets and deployed credentials in the hosting provider's secret store. Do not commit populated credential fields in appsettings files, environment files, private keys, or generated bin/obj directories.

The API requires an externally configured JWT signing key. Generate the administrator's BCrypt hash using the interactive HashGen utility; do not put a plaintext administrator password in source code. Integration tests use a process-local signing key and their own test configuration.

## If a credential was committed

1. Revoke or rotate it at its provider before publishing the repository. Deleting a file does not invalidate a credential.
2. Update the application's secret store. Changing the JWT key invalidates existing JWTs and signed tracking links. Change the administrator password as well as its stored hash.
3. Remove the credential from every affected branch, tag, and historical commit, including generated configuration copies and compiled artifacts.
4. Coordinate a history rewrite with anyone holding a clone. Old clones can reintroduce the affected commits.
5. Review pull request references, cached diffs, Actions logs/artifacts, and releases. GitHub-managed pull request references cannot be removed by an ordinary Git push; repository history cleanup alone does not remove all hosted copies. Contact GitHub Support when sensitive-data removal is required.

Do not post credentials in an issue, pull request, screenshot, or security report. Include only the affected file paths and credential types.

## Scan with Gitleaks

From the repository root, with Gitleaks installed:

```bash
gitleaks git . --log-opts="--all" --config=.gitleaks.toml --redact=100
```

This examines local Git references. Fetch all branches/tags and review GitHub-managed references separately when preparing a repository for publication. A clean scanner report does not replace manual review or credential rotation.
