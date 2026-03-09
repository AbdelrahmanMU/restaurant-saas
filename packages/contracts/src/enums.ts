export enum Role {
  Owner = 'Owner',
  RestaurantManager = 'RestaurantManager',
  BranchManager = 'BranchManager',
  Cashier = 'Cashier',
  Coordinator = 'Coordinator',
  Driver = 'Driver'
}

export enum OrderStatus {
  PendingAcceptance = 'PendingAcceptance',
  Accepted = 'Accepted',
  Preparing = 'Preparing',
  ReadyForDispatch = 'ReadyForDispatch',
  PendingHandover = 'PendingHandover',
  PickedUp = 'PickedUp',
  Delivered = 'Delivered',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum FulfillmentType {
  Delivery = 'Delivery',
  Pickup = 'Pickup',
  DineIn = 'DineIn'
}
