## Prefab Palette: Contributing

---
## See Also
* [ReadMe](./README.md)
* [Developers](./Developers.md) 
---

Contributions are encouraged, whether fixing bugs, adding new modes, or improving documentation, your help is appreciated. 

## How to contribute

1. **Read the Docs:** Familiarise yourself with the contribution guide and the  [Developers](./Developers.md) doc. 

1. **Create or Claim an issue (If applicable):** Before starting significant work (e.g. new modes, features or bug fixes), it’s recommended to check the issues page to see if someone is already working on it. You can claim an open issue, or create a new one. This helps avoid duplicate work and allows for early feedback.

2. **Fork the repository:** Fork the Prefab Palette repository to your github account.

3. **Create a new branch:** For each new feature or bugfix, create a new branch from main. Use descriptive names for your branches (e.g., feature/add-new-mode, fix/bug-or-issue).

4. **Write clear commit messages:** Use concise and informative commit messages that explain the purpose of your changes.

5. **Submit a Pull Request (PR):** Once you’ve tested your changes, submit a pull request to the main branch of the original Prefab Palette repository.

## Contribution Guidelines

To ensure smooth collaboration and maintainability, please follow the guidelines outlined in this section.

### Code Style

Please adhere to the existing code style within the project. Consistency helps maintain readability and helps simplify future maintenance.

### Editor Folder Structure

When adding new files, please respect the established folder structure within the Editor directory.

### New Placement Modes

If you’re creating a new placement mode, follow the steps outlined in the **Modes** section of the [Developers](./Developers.md) doc, this ensures your mode integrates seamlessly with the existing architecture.

Please ensure the following is adhered to:

1. Both the Mode and Settings script are in the correct folders.  
2. The modes settings asset is correctly generated/loaded in the appropriate subfolder within the `Generated` folder, using `PathDr` for path management.  
3. The new modes toolbar icon is setup correctly.
4. Any instantiated prefabs are integrated into Unity’s native Undo/Redo system.  

### Path Management

The `PathDr` script manages valid paths to the *Generated*, *Collections*, and *Mode Settings* folder. When interacting with the tools file structure, please ensure you use the appropriate properties from `PathDr` to retrieve or construct paths.

### Documentation

If your contribution introduces new features or changes existing behavior, please update the relevant sections of the docs to reflect the changes.

### Versioning

Prefab Palette follows [Semantic Versioning](https://semver.org/) — `MAJOR.MINOR.PATCH`.

Please update the version appropriately if your contribution changes behavior or functionality:

- **MAJOR** – Incompatible API or system changes.
- **MINOR** – New features or functionality (backwards-compatible).
- **PATCH** – Bug fixes, internal refactors, or documentation updates.

> ✅ Example: If you add a new placement mode without breaking anything else, bump from `1.2.0` to `1.3.0`.

⚠️ Note: Version tags Should be included in pull request titles. Update the [Changelog](./Changelog.md) file as part of your PR.

---

## Distribution

Prefab Palette is distributed as a `.unitypackage` in the Releases section of the repo:

### Prepare for Export

1. Empty the collections folder and reset the enum by removing all items from the list in the `PrefabCollectionsManager > Collections Inspector` window.
2. Open `PrefabPalette/Editor/Generated`. 
3. Delete everything except `CollectionName.cs`
4. Ensure no prefabs or other assets are contained in the release.

### Export as `.unitypackage`
1. In the **Project** window, select the `PrefabPalette` folder.
2. Right-click the selection and choose **Export Package...**
3. In the export dialog:
   - Deselect **Include dependencies**
   - Deselect **Include all scripts**
   - Deselect `PrefabPalette/Docs` folder.
4. Click **Export**, then name your file:  
   `PrefabPalette_vX.Y.Z.unitypackage`

> Attach this package to a new github release with the same version tag.

⚠️ Note: Ensure no prefabs or other assets are contained in the release!
