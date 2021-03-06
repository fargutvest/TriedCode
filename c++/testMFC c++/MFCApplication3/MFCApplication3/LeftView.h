// Этот исходный код MFC Samples демонстрирует функционирование пользовательского интерфейса Fluent на основе MFC в Microsoft Office
// ("Fluent UI") и предоставляется исключительно как справочный материал в качестве дополнения к
// справочнику по пакету Microsoft Foundation Classes и связанной электронной документации,
// включенной в программное обеспечение библиотеки MFC C++.  
// Условия лицензионного соглашения на копирование, использование или распространение Fluent UI доступны отдельно.  
// Для получения дополнительных сведений о нашей лицензионной программе Fluent UI посетите веб-узел
// http://go.microsoft.com/fwlink/?LinkId=238214.
//
// (C) Корпорация Майкрософт (Microsoft Corp.)
// Все права защищены.

// LeftView.h : интерфейс класса CLeftView
//


#pragma once

class CMFCApplication3Doc;

class CLeftView : public CTreeView
{
protected: // создать только из сериализации
	CLeftView();
	DECLARE_DYNCREATE(CLeftView)

// Атрибуты
public:
	CMFCApplication3Doc* GetDocument();

// Операции
public:

// Переопределение
	public:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	protected:
	virtual void OnInitialUpdate(); // вызывается в первый раз после конструктора

// Реализация
public:
	virtual ~CLeftView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Созданные функции схемы сообщений
protected:
	DECLARE_MESSAGE_MAP()
};

#ifndef _DEBUG  // отладочная версия в LeftView.cpp
inline CMFCApplication3Doc* CLeftView::GetDocument()
   { return reinterpret_cast<CMFCApplication3Doc*>(m_pDocument); }
#endif

