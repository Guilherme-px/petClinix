# PetClinix

PetClinix is a multi-tenant SaaS MVP for veterinary clinics, designed and implemented as a public portfolio project to demonstrate senior-level software engineering practices, product thinking, and architectural decision-making.

This repository intentionally represents the **public MVP stage** of the product.  
Its purpose is to showcase the foundation of the platform, including domain modeling, modular architecture, tenancy boundaries, and the first core business capabilities.

The full product evolution, including advanced features, production hardening, and commercial-grade improvements, is intended to continue in a **private repository** after the MVP milestone.

---

## Purpose

This project was created to demonstrate the design and implementation of a real-world SaaS product with a strong focus on:

- Clean Architecture
- Domain-Driven Design (DDD)
- modular design
- multi-tenant thinking
- maintainability
- scalability
- clear separation of concerns
- domain-first software design

Rather than building a generic CRUD application, the goal is to model a realistic business domain and build a solid architectural foundation that can support future growth.

---

## Product Vision

PetClinix aims to provide veterinary clinics with a centralized platform to manage their daily operations, staff, pets, tutors, scheduling, subscriptions, and clinical workflows.

The platform is being designed as a SaaS product where each clinic operates within its own isolated data scope.

At the MVP stage, the product focuses on validating the core operational foundation of the platform.

---

## Public MVP Scope

The public MVP is intended to cover the core foundations and essential workflows of the platform, including:

- clinic onboarding and registration
- subscription signup and payment flow
- authentication
- employee management
- pet registration
- tutor registration
- schedule management
- services management
- vaccine tracking

This scope is intentionally broad enough to represent a realistic vertical slice of the product, while still being limited to an MVP that is suitable for public demonstration.

---

## Repository Strategy

This repository is the **public MVP showcase**.

Its purpose is to demonstrate:

- architectural foundations
- domain design
- project structure
- coding standards
- technical direction
- implementation style

After this MVP reaches a representative and demonstrable state, the project will continue evolving in a **private repository**, where the product can be further refined with:

- additional modules
- production-grade hardening
- deeper business rules
- improved UX and operational concerns
- infrastructure evolution
- commercial and security enhancements

This public version should therefore be seen as a **portfolio-grade architectural milestone**, not the final commercial form of the product.

---

## Architecture

PetClinix is being built as a **modular monolith** with clear dependency boundaries and business-oriented module separation.

The solution follows the principles of:

- Clean Architecture
- Domain-Driven Design
- modular organization by business context

### Current project structure

```text
PetClinix.Api
PetClinix.BuildingBlocks.Application
PetClinix.BuildingBlocks.Domain
PetClinix.BuildingBlocks.Infrastructure
PetClinix.Modules.Identity.Application
PetClinix.Modules.Identity.Domain
PetClinix.Modules.Identity.Infrastructure