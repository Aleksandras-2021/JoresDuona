const apiBaseUrl = "/api/Discount";

// Fetch and display discounts
async function fetchDiscounts() {
    const response = await fetch(apiBaseUrl);
    const discounts = await response.json();
    populateDiscountTable(discounts);
}

// Populate the table with discounts
function populateDiscountTable(discounts) {
    const tbody = document.querySelector("table tbody");
    tbody.innerHTML = discounts.map(discount => `
        <tr>
            <td>${discount.description}</td>
            <td>${discount.amount} ${discount.isPercentage ? "%" : ""}</td>
            <td>${discount.isPercentage ? "Percentage" : "Fixed"}</td>
            <td>${new Date(discount.validFrom).toLocaleDateString()}</td>
            <td>${new Date(discount.validTo).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-warning btn-sm" onclick="editDiscount(${discount.id})">Edit</button>
                <button class="btn btn-danger btn-sm" onclick="deleteDiscount(${discount.id})">Delete</button>
            </td>
        </tr>
    `).join("");
}

// Add a new discount
async function addDiscount() {
    const description = prompt("Enter discount description:");
    if (!description) return;

    const amount = parseFloat(prompt("Enter discount amount:"));
    if (isNaN(amount)) return alert("Amount must be a number!");

    const isPercentage = confirm("Is this a percentage discount?");
    const validFrom = prompt("Enter start date (YYYY-MM-DD):");
    const validTo = prompt("Enter end date (YYYY-MM-DD):");

    const discount = { description, amount, isPercentage, validFrom, validTo };

    const response = await fetch(apiBaseUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(discount)
    });

    if (response.ok) {
        fetchDiscounts();
    } else {
        alert("Failed to add discount.");
    }
}

// Edit a discount
async function editDiscount(id) {
    const response = await fetch(`${apiBaseUrl}/${id}`);
    const discount = await response.json();

    discount.description = prompt("Edit description:", discount.description);
    discount.amount = parseFloat(prompt("Edit amount:", discount.amount));
    discount.isPercentage = confirm("Is this a percentage discount?");
    discount.validFrom = prompt("Edit start date (YYYY-MM-DD):", discount.validFrom);
    discount.validTo = prompt("Edit end date (YYYY-MM-DD):", discount.validTo);

    const updateResponse = await fetch(`${apiBaseUrl}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(discount)
    });

    if (updateResponse.ok) {
        fetchDiscounts();
    } else {
        alert("Failed to edit discount.");
    }
}

// Delete a discount
async function deleteDiscount(id) {
    if (confirm("Are you sure you want to delete this discount?")) {
        const response = await fetch(`${apiBaseUrl}/${id}`, { method: "DELETE" });

        if (response.ok) {
            fetchDiscounts();
        } else {
            alert("Failed to delete discount.");
        }
    }
}

// Initialize table on page load
document.addEventListener("DOMContentLoaded", fetchDiscounts);
