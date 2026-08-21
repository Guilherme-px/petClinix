[🇧🇷 Português](GUIDE.pt-br.md) | [🇺🇸 English](GUIDE.md) | [📜 Voltar para o README](README.pt-br.md)

# 🧭 PetClinix API - Guia de Testes

Este guia fornece um passo a passo para testar todo o fluxo do SaaS usando a interface do Swagger UI. 
Você pode seguir estes passos localmente ou usando nossa [Live Demo](https://petclinix.onrender.com/swagger/index.html).

> **Nota sobre E-mails (Modo Demo):** Como usamos o plano gratuito para e-mails (Resend), os e-mails reais são enviados apenas para o e-mail verificado do dono do projeto. Se você usar seu próprio e-mail durante o cadastro, o sistema gerará um token de redefinição de senha e o retornará diretamente na resposta da API para que você possa continuar testando sem esperar por um e-mail.

---

### Passo 1: Cadastro da Clínica
Cadastre uma nova clínica e seu usuário Admin inicial.

1. Acesse o endpoint `POST /api/clinics`.
2. Clique em **"Try it out"**.
3. Cole o seguinte JSON (substitua o `adminEmail` pelo seu e-mail real se quiser testar o recebimento):
```json
{
  "tradeName": "Clínica Teste Guia",
  "legalName": "Teste Guia LTDA",
  "documentNumber": "12345678000199",
  "email": "contato@clinicateste.com",
  "phoneNumber": "11988887777",
  "zipCode": "01001000",
  "street": "Avenida Paulista",
  "number": "1000",
  "neighborhood": "Bela Vista",
  "complement": "Sala 12",
  "city": "São Paulo",
  "state": "SP",
  "adminName": "Admin Guia",
  "adminEmail": "seu.email@exemplo.com",
  "adminDocumentNumber": "12345678900",
  "adminPhoneNumber": "11999990000",
  "adminBirthDate": "1990-05-15"
}
```
4. Clique em **Execute**.
5. **Verifique a Resposta:** Você receberá um `200 OK` contendo um JSON com `clinicId`, `adminUserId` e `passwordResetToken`.
6. **Copie o `passwordResetToken`** e o `clinicId`.

---

### Passo 2: Definir Senha do Admin
O admin foi criado sem senha. Defina-a para habilitar o login.

1. Acesse o endpoint `POST /api/users/set-password`.
2. Clique em **"Try it out"**.
3. Cole o seguinte JSON (substitua o token pelo que você copiou no Passo 1):
```json
{
  "token": "COLE_SEU_TOKEN_AQUI",
  "password": "SenhaForte@123"
}
```
4. Clique em **Execute**. Você deverá receber um `200 OK` com uma mensagem de sucesso.

---

### Passo 3: Login e Autorização no Swagger
Autentique-se para acessar as rotas protegidas.

1. Acesse o endpoint `POST /api/users/login`.
2. Clique em **"Try it out"**.
3. Cole o seguinte JSON:
```json
{
  "email": "seu.email@exemplo.com",
  "password": "SenhaForte@123"
}
```
4. Clique em **Execute**. Você receberá um `200 OK` com seu JWT Token e o Refresh Token.
5. **Copie o `token`** (a string longa).
6. Vá para o topo da página do Swagger e clique no botão **Authorize** (🔒).
7. Cole seu token e clique em **Authorize**. Agora você está autenticado!

---

### Passo 4: Ativar Assinatura (Stripe Checkout)
Teste o fluxo de pagamento de assinatura usando o Stripe Test Mode. A assinatura deve estar ativa para que a clínica use o sistema por completo.

1. Acesse o endpoint `POST /api/subscriptions/checkout`.
2. Use o `priceId` abaixo (configurado no ambiente de desenvolvimento) e o `clinicId` do Passo 1.
```json
{
  "clinicId": "SEU_CLINIC_ID",
  "priceId": "price_1TvDgvDF8QReEmHqNAvLYI78",
  "adminEmail": "seu.email@exemplo.com",
  "successUrl": "http://localhost:3000/success",
  "cancelUrl": "http://localhost:3000/cancel"
}
```
3. Clique em **Execute**. A API retornará uma `checkoutUrl`.
4. Abra a `checkoutUrl` no seu navegador.
5. Use os números de cartões de teste do Stripe para concluir o pagamento. Você pode encontrá-los aqui: [Cartões de Teste do Stripe](https://docs.stripe.com/testing?testing-method=card-numbers). (Ex: Número do cartão: `4242 4242 4242 4242`, qualquer data futura, qualquer CVC).
6. Após o pagamento, o Stripe enviará um webhook para a API, ativando a assinatura da clínica e disparando o e-mail de boas-vindas/definição de senha (ou retornando o token se estiver em Modo Demo).

---

### Passo 5: Cadastrar um Serviço
Adicione um serviço que a clínica oferecerá.

1. Acesse o endpoint `POST /api/services`.
2. Cole o seguinte JSON:
```json
{
  "name": "Consulta Clínica Geral",
  "description": "Consulta padrão com veterinário.",
  "durationInMinutes": 30,
  "price": 150.00,
  "requiresVeterinarian": true
}
```
3. Clique em **Execute**. Você deverá receber um `204 No Content`.
4. Para obter o `ServiceId`, acesse `GET /api/services` e clique em Execute. Copie o `id` do serviço que você acabou de criar.

---

### Passo 6: Cadastrar Funcionário (Veterinário)
Cadastre um veterinário. Este endpoint também retorna um token na resposta para que o veterinário possa definir sua senha depois.

1. Acesse o endpoint `POST /api/clinics/me/staff`.
2. Cole o seguinte JSON:
```json
{
  "name": "Dr. Dolittle",
  "email": "dr.dolittle@exemplo.com",
  "documentNumber": "98765432100",
  "phoneNumber": "11988887777",
  "birthDate": "1985-05-10",
  "role": "Veterinarian"
}
```
3. Clique em **Execute**. Você receberá um `200 OK` contendo o `userId` e um `passwordResetToken` para o veterinário. **Copie o `userId`**.

---

### Passo 7: Cadastrar Tutor e Pet
Cadastre um cliente e seu pet.

1. Acesse `POST /api/tutors` e clique em Execute:
```json
{
  "name": "João da Silva",
  "cpf": "11122233344",
  "email": "joao.silva@exemplo.com",
  "phoneNumber": "11999998888",
  "zipCode": "01001000",
  "street": "Rua das Flores",
  "number": "123",
  "neighborhood": "Centro",
  "city": "São Paulo",
  "state": "SP"
}
```
2. Você receberá um `204 No Content`.
3. Para obter o ID do Tutor, acesse `GET /api/tutors` e clique em Execute. **Copie o `id`** do tutor que você acabou de criar.
4. Agora, acesse `POST /api/tutors/{tutorId}/pets`. Substitua o `{tutorId}` na URL pelo ID que você copiou. Execute:
```json
{
  "name": "Rex",
  "species": 1,
  "breed": "Vira-lata",
  "birthDate": "2020-05-10",
  "sex": 1,
  "weight": 15.5,
  "isNeutered": true,
  "notes": "Agressivo com outros machos"
}
```
5. Você receberá um `204 No Content`.
6. Para obter o ID do Pet, acesse `GET /api/tutors/{tutorId}/pets` (usando o ID do Tutor). **Copie o `id` do pet**.

---

### Passo 8: Agendar uma Consulta
Agende uma consulta para o pet.

1. Acesse o endpoint `GET /api/appointments/available-slots`.
2. Forneça o `VetId` (o `userId` do Passo 6), o `ServiceId` (do Passo 5) e uma `date` no futuro (ex: `2026-12-15`).
3. Clique em **Execute**. A API retornará um array de horários disponíveis, como `["08:00", "08:30", "09:00"]`.
4. Agora, acesse o endpoint `POST /api/appointments`. 
5. Use o TutorId (Passo 7), PetId (Passo 7), ServiceId (Passo 5) e VetId (Passo 6). Para o `scheduledDateUtc`, combine a data com um dos horários disponíveis (ex: `2026-12-15T09:00:00Z`).
```json
{
  "tutorId": "SEU_TUTOR_ID",
  "petId": "SEU_PET_ID",
  "serviceId": "SEU_SERVICE_ID",
  "veterinarianId": "SEU_VET_ID",
  "scheduledDateUtc": "2026-12-15T09:00:00Z",
  "notes": "Primeira consulta"
}
```
6. Clique em **Execute**. Você receberá um `204 No Content`. A consulta foi agendada!