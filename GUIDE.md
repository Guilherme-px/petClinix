[🇧🇷 Português](GUIDE.pt-br.md) | [🇺🇸 English](GUIDE.md) | [📜 Back to README](README.md)

# 🧭 PetClinix API - Testing Guide

This guide provides a step-by-step walkthrough to test the entire SaaS flow using the Swagger UI. 
You can follow these steps locally or using our [Live Demo](https://petclinix.onrender.com/swagger/index.html).

> **Note on Emails (Demo Mode):** Because we use a free tier for emails (Resend), actual emails are only sent to the project owner's verified email. If you use your own email during registration, the system will generate a password reset token and return it directly in the API response so you can continue testing without waiting for an email.

---

### Step 1: Clinic Onboarding
Register a new clinic and its initial Admin user.

1. Go to `POST /api/clinics`.
2. Click **"Try it out"**.
3. Paste the following JSON (replace `adminEmail` with your real email if you want to test the actual email delivery):
```json
{
  "tradeName": "Test Guide Clinic",
  "legalName": "Test Guide LLC",
  "documentNumber": "12345678000199",
  "email": "contact@testguideclinic.com",
  "phoneNumber": "11988887777",
  "zipCode": "01001000",
  "street": "Paulista Avenue",
  "number": "1000",
  "neighborhood": "Bela Vista",
  "complement": "Suite 12",
  "city": "Sao Paulo",
  "state": "SP",
  "adminName": "Guide Admin",
  "adminEmail": "your.email@example.com",
  "adminDocumentNumber": "12345678900",
  "adminPhoneNumber": "11999990000",
  "adminBirthDate": "1990-05-15"
}
```
4. Click **Execute**.
5. **Check the Response:** You will get a `200 OK` containing a JSON with `clinicId`, `adminUserId`, and `passwordResetToken`.
6. **Copy the `passwordResetToken`** and the `clinicId`.

---

### Step 2: Set Admin Password
The admin was created without a password. Define it to enable login.

1. Go to `POST /api/users/set-password`.
2. Click **"Try it out"**.
3. Paste the following JSON (replace the token with the one you copied in Step 1):
```json
{
  "token": "PASTE_YOUR_TOKEN_HERE",
  "password": "SenhaForte@123"
}
```
4. Click **Execute**. You should receive a `200 OK` with a success message.

---

### Step 3: Login & Authorize
Authenticate to access protected routes.

1. Go to `POST /api/users/login`.
2. Click **"Try it out"**.
3. Paste the following JSON:
```json
{
  "email": "your.email@example.com",
  "password": "SenhaForte@123"
}
```
4. Click **Execute**. You will receive a `200 OK` with your JWT Token and Refresh Token.
5. **Copy the `token`** (the long string).
6. Scroll to the top of the Swagger page and click the **Authorize** button (🔒).
7. Paste your token and click **Authorize**. Now you are authenticated!

---

### Step 4: Activate Subscription (Stripe Checkout)
Test the subscription payment flow using Stripe Test Mode. The subscription must be active for the clinic to fully use the system.

1. Go to `POST /api/subscriptions/checkout`.
2. Use the `priceId` below (configured in the dev environment) and the `clinicId` from Step 1.
```json
{
  "clinicId": "YOUR_CLINIC_ID",
  "priceId": "price_1TvDgvDF8QReEmHqNAvLYI78",
  "adminEmail": "your.email@example.com",
  "successUrl": "http://localhost:3000/success",
  "cancelUrl": "http://localhost:3000/cancel"
}
```
3. Click **Execute**. The API will return a `checkoutUrl`.
4. Open the `checkoutUrl` in your browser.
5. Use Stripe's test card numbers to complete the payment. You can find them here: [Stripe Test Cards](https://docs.stripe.com/testing?testing-method=card-numbers). (e.g., Card number: `4242 4242 4242 4242`, any future date, any CVC).
6. After payment, Stripe will send a webhook to the API, activating the clinic's subscription and triggering the welcome/password setup email (or returning the token if in Demo Mode).

---

### Step 5: Register a Service
Add a service that the clinic will offer.

1. Go to `POST /api/services`.
2. Paste the following JSON:
```json
{
  "name": "General Clinical Consultation",
  "description": "Standard consultation with a veterinarian.",
  "durationInMinutes": 30,
  "price": 150.00,
  "requiresVeterinarian": true
}
```
3. Click **Execute**. You should receive a `204 No Content`.
4. To get the `ServiceId`, go to `GET /api/services` and execute it. Copy the `id` of the service you just created.

---

### Step 6: Register Staff (Veterinarian)
Register a veterinarian. This endpoint also returns a token in the response so the vet can set their password later.

1. Go to `POST /api/clinics/me/staff`.
2. Paste the following JSON:
```json
{
  "name": "Dr. Dolittle",
  "email": "dr.dolittle@example.com",
  "documentNumber": "98765432100",
  "phoneNumber": "11988887777",
  "birthDate": "1985-05-10",
  "role": "Veterinarian"
}
```
3. Click **Execute**. You will get a `200 OK` containing the `userId` and a `passwordResetToken` for the vet. **Copy the `userId`**.

---

### Step 7: Register Tutor & Pet
Register a customer and their pet.

1. Go to `POST /api/tutors` and execute:
```json
{
  "name": "John Doe",
  "cpf": "11122233344",
  "email": "john.doe@example.com",
  "phoneNumber": "11999998888",
  "zipCode": "01001000",
  "street": "Flowers Street",
  "number": "123",
  "neighborhood": "Downtown",
  "city": "Sao Paulo",
  "state": "SP"
}
```
2. You will get a `204 No Content`.
3. To get the Tutor's ID, go to `GET /api/tutors` and execute it. **Copy the `id`** of the tutor you just created.
4. Now, go to `POST /api/tutors/{tutorId}/pets`. Replace `{tutorId}` in the URL with the ID you copied. Execute:
```json
{
  "name": "Rex",
  "species": 1,
  "breed": "Mixed Breed",
  "birthDate": "2020-05-10",
  "sex": 1,
  "weight": 15.5,
  "isNeutered": true,
  "notes": "Aggressive towards other males"
}
```
5. You will get a `204 No Content`.
6. To get the Pet's ID, go to `GET /api/tutors/{tutorId}/pets` (using the Tutor ID). **Copy the pet `id`**.

---

### Step 8: Schedule an Appointment
Book an appointment.

1. Go to `GET /api/appointments/available-slots`.
2. Provide the `VetId` (the `userId` from Step 6), the `ServiceId` (from Step 5), and a `date` in the future (e.g., `2026-12-15`).
3. Click **Execute**. The API will return an array of available times like `["08:00", "08:30", "09:00"]`.
4. Now, go to `POST /api/appointments`. 
5. Use the TutorId (Step 7), PetId (Step 7), ServiceId (Step 5), and VetId (Step 6). For `scheduledDateUtc`, combine the date with one of the available times (e.g., `2026-12-15T09:00:00Z`).
```json
{
  "tutorId": "YOUR_TUTOR_ID",
  "petId": "YOUR_PET_ID",
  "serviceId": "YOUR_SERVICE_ID",
  "veterinarianId": "YOUR_VET_ID",
  "scheduledDateUtc": "2026-12-15T09:00:00Z",
  "notes": "First consultation"
}
```
6. Click **Execute**. You will get a `204 No Content`. The appointment is booked!