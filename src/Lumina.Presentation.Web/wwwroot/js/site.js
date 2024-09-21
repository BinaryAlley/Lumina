let windowWasClicked = false;

/**
 * Adjusts the height of a dropdown identified by `comboboxId` to fit in the available space.
 * @param {string} comboboxId The Id of the combobox whose dropdown should be adjusted.
 */
function adjustDropdownHeight(comboboxId = null) {
    const comboboxes = comboboxId
        ? [document.getElementById(comboboxId)] // if comboboxId is provided, use only that element
        : document.querySelectorAll('[id^="combobox_"]:not([id^="combobox_checkbox_"])'); // otherwise, select all dropdowns
    if (comboboxes) {
        comboboxes.forEach(combobox => {
            if (!combobox) return; // skip if no element is found
            const dropdown = combobox.querySelector('.enlightenment-dropdown');
            if (!dropdown) return; // skip if there's no dropdown inside
            // get the bounds of the combobox and the height of the viewport
            const comboboxRect = combobox.getBoundingClientRect();
            const viewportHeight = window.innerHeight;
            // calculate the space available between the combobox and the bottom of the viewport, with a 10px margin
            const dropdownTopPosition = comboboxRect.top + comboboxRect.height;
            const availableSpace = viewportHeight - dropdownTopPosition - 10; // 10px margin
            // adjust the dropdown height
            dropdown.style.maxHeight = `${availableSpace}px`;
        });
    }
}

/**
 * Gets the position and size of an element relative to the viewport.
 * @param {string} id - The Id of the element to get the offset for.
 * @returns {Object} An object with the top, left, width, and height of the element.
 */
function getElementOffset(id) {
    const element = document.getElementById(id);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            top: rect.top + window.scrollY,
            left: rect.left + window.scrollX,
            width: rect.width,
            height: rect.height
        };
    }
    return {
        top: 0,
        left: 0,
        width: 0,
        height: 0
    };
}

/**
 * Updates the position and height of the dropdown relative to the specified path segment.
 * @param {string} pathSegmentId - The ID of the path segment element to position the dropdown relative to.
 */
function updateDropdownPosition(pathSegmentId) {
    const element = document.getElementById(pathSegmentId);
    const dropdown = document.getElementById('navigatorDropdown');
    if (element && dropdown) {
        const rect = getElementOffset(pathSegmentId);
        dropdown.style.top = `${rect.top + rect.height}px`;
        dropdown.style.left = `${rect.left}px`;
        // calculate the available height between the dropdown and the bottom of the viewport
        const viewportHeight = window.innerHeight;
        const availableHeight = viewportHeight - (rect.top + rect.height) - 10; // 10 px margin
        // set the dropdown's max height, ensuring it doesn't exceed the available height
        dropdown.style.maxHeight = `${availableHeight}px`;
    }
}

/**
 * Enables horizontal scrolling to the file system browser explorer tab.
 */
function addHorizontalScrolling(id) {
    document.getElementById(id).addEventListener('wheel', function (event) {
        event.preventDefault();
        this.scrollLeft += (event.deltaY > 0 ? 1 : -1) * 80;
    }, { passive: false });
}

/**
 * Disables horizontal scrolling to the file system browser explorer tab.
 */
function removeHorizontalScrolling(id) {
    document.getElementById(id).removeEventListener('wheel', function (event) {
        event.preventDefault();
        this.scrollLeft += (event.deltaY > 0 ? 1 : -1) * 80;
    }, { passive: false });
}

/**
 * Focuses the navigator address bar input.
 */
function focusNavigatorInputElement() {
    const elements = document.getElementsByClassName('navigator-address-bar-input');
    if (elements.length > 0)
        elements[0].focus();
}

/**
 * Unchecks all checkboxes except the one with the specified Id.
 * @param {string} id - The Id of the checkbox to leave checked.
 */
function uncheckAllExcept(id) {
    const checkboxes = document.querySelectorAll('.navigator-toggle-checkbox');
    checkboxes.forEach((checkbox) => {
        if (checkbox.id !== id)
            checkbox.checked = false;
    });
}

/**
 * Closes all navigator path segments.
 */
function closeNavigatorPathSegments() {
    const checkboxes = document.getElementsByClassName('navigator-toggle-checkbox');
    for (let i = 0; i < checkboxes.length; i++)
        checkboxes[i].checked = false;
}

/**
 * Handles the window resize event.
 */
window.addEventListener('resize', function () {
    adjustDropdownHeight();
    // update all dropdowns' positions on resize
    const dropdown = document.getElementById('navigatorDropdown');
    if (dropdown) {
        const pathSegmentId = dropdown.getAttribute('data-path-segment-id');
        if (pathSegmentId)
            updateDropdownPosition(pathSegmentId);
    }
});

/**
 * Attaches a click event listener to the window, invoking a .NET method when the window is clicked.
 * @param {any} dotnetHelper - .NET object to invoke methods asynchronously.
 */
window.windowClickHandler = (dotnetHelper) => {
    window.addEventListener('click', (event) => {
        dotnetHelper.invokeMethodAsync('OnWindowClick', {
            x: event.clientX,
            y: event.clientY,
            sender: event.target.getAttribute('id')
        });
    });
};

/**
 * Attaches a keydown event listener to the given element, invoking a .NET method when 'Escape' is pressed.
 * @param {HTMLElement} element - The HTML element to attach the keydown listener to.
 * @param {any} dotnetHelper - .NET object to invoke methods asynchronously.
 */
window.windowKeyDownHandler = (element, dotnetHelper) => {
    element.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            e.preventDefault();
            dotnetHelper.invokeMethodAsync('EscapePressed');
        }
    });
};