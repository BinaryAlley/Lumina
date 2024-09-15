/**
 * Adjusts the height of a dropdown identified by `comboboxId` to fit in the available space.
 * @param {string} comboboxId The ID of the combobox whose dropdown should be adjusted.
 */
function adjustDropdownHeight(comboboxId) {
    const combobox = document.getElementById(comboboxId);
    const dropdown = combobox.querySelector('.enlightenment-dropdown');
    // get the bounds of the combobox and the height of the viewport
    const comboboxRect = combobox.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    // calculate the space available between the combobox and the bottom of the viewport, with a 10px margin
    const dropdownTopPosition = comboboxRect.top + comboboxRect.height;
    const availableSpace = viewportHeight - dropdownTopPosition - 10; // 10px margin
    dropdown.style.maxHeight = `${availableSpace}px`;
}

window.showDirectoryPicker = async () => {
    try {
        const dirHandle = await window.showDirectoryPicker().ConfigureAwait(false);
        return dirHandle.name;
    } catch (err) {
        console.error(err);
        return null;
    }
};