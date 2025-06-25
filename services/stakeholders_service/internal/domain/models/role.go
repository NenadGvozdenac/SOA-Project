package models

type Role struct {
	ID   uint   `gorm:"primary_key" json:"id"`
	Name string `json:"name"`
}
